using UnityEngine;
using System;
using Mirror;
using System.Collections;
using Enums;
using NeuroRehab.Mappings;

public class AnimationServerManager : NetworkBehaviour {

	private AnimationSettingsManager animSettingsManager;

	private static AnimationServerManager _instance;

	public static AnimationServerManager Instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<AnimationServerManager>();
			}
			return _instance;
		}
	}

	public bool isTrainingRunning = false;
	private bool isAnimationTriggered = false;
	private DateTime lastAnimationTrigger;
	private int currentRepetitions = 0;
	private Coroutine trainingCoroutine;

	public override void OnStartServer() {
		animSettingsManager = gameObject.GetComponent<AnimationSettingsManager>();
	}

	private IEnumerator waitDurationCoroutine(float duration) {
		yield return new WaitForSeconds(duration);
		if (!isTrainingRunning) {
			yield break;
		}
		isTrainingRunning = false;
		isAnimationTriggered = false;

		Debug.Log("Listening to animation events - Canceled");
		yield return new WaitForSecondsRealtime(1f);
		RpcStopTraining("Training cancelled - no activity from patient", MessageType.WARNING);
	}

	/// <summary>
	/// Starts animation if it's in time and we still have repetitions left
	/// </summary>
	/// <returns>Whether move was succesfully triggered</returns>
	[Server]
	public bool moveArm() {
		//Debug.Log("Move arm called");
		if (!isAnimationTriggered && isTrainingRunning) {
			DateTime currentTime = DateTime.Now;
			Debug.Log("Seconds since last animation step: " + (currentTime - lastAnimationTrigger).TotalSeconds);
			if ((currentTime - lastAnimationTrigger).TotalSeconds <= animSettingsManager.waitDuration) {
				Debug.Log("Starting arm animation");
				RpcStartActualAnimation(false, (currentRepetitions + 1) + "/" + animSettingsManager.repetitions);
				isAnimationTriggered = true;

				if (trainingCoroutine != null) {
					StopCoroutine(trainingCoroutine);
				}
				return true;
			} else {
				isTrainingRunning = false;
				isAnimationTriggered = false;

				RpcStopTraining();
			}
		}
		return false;
	}

	/// <summary>
	/// Marks animation step as done
	/// </summary>
	[Server]
	public void progressAnimationStep() {
		if (!isTrainingRunning) {
			return;
		}

		// Debug.Log("animation step received");
		DateTime currentTime = DateTime.Now;
		lastAnimationTrigger = currentTime;
		currentRepetitions++;
		if (currentRepetitions >= animSettingsManager.repetitions) {
			isTrainingRunning = false;

			RpcStopTraining();
		} else {
			RpcStartCountdown(animSettingsManager.waitDuration, currentRepetitions + "/" + animSettingsManager.repetitions);
		}

		trainingCoroutine = StartCoroutine(waitDurationCoroutine(animSettingsManager.waitDuration));

		isAnimationTriggered = false;
	}

	/// <summary>
	/// Starts listening to animation move events
	/// </summary>
	/// <returns>Whether we succesfully started listening to new moves</returns>
	[Server]
	public bool startTraining() {
		if (!canTrainingStart()) {
			return false;
		}

		Debug.Log("Listening to animation events - STARTED");
		isTrainingRunning = true;
		isAnimationTriggered = false;
		lastAnimationTrigger = DateTime.Now;
		currentRepetitions = 0;

		RpcStartTraining(animSettingsManager.waitDuration, "0/" + animSettingsManager.repetitions);
		if (trainingCoroutine != null) {
			StopCoroutine(trainingCoroutine);
		}
		trainingCoroutine = StartCoroutine(waitDurationCoroutine(animSettingsManager.waitDuration));

		return true;
	}

	[Server]
	private bool canTrainingStart() {
		if (CharacterManager.activePatientInstance == null) {
			Debug.LogError("No patient present! Can't start training.");
			MessageManager.Instance.RpcInformClients("No patient present! Can't start training.", MessageType.WARNING);
			return false;
		}
		if (isTrainingRunning) {
			DateTime currentTime = DateTime.Now;
			if ((currentTime - lastAnimationTrigger).TotalSeconds <= animSettingsManager.waitDuration) {
				Debug.LogError("Training already started! Can't start new training.");
				MessageManager.Instance.RpcInformClients("Training already started! Can't start new training.", MessageType.WARNING);
				return false;
			}
		}

		if (animSettingsManager.animType == AnimationType.Off) {
			Debug.LogError("No animationy type chosen! Can't start training.");
			MessageManager.Instance.RpcInformClients("No animationy type chosen! Can't start training.", MessageType.WARNING);
			return false;
		}

		SyncList<PosRotMapping> currentAnimationSetup = animSettingsManager.getCurrentAnimationSetup();
		if (currentAnimationSetup.Count < 1) {
			string errorMessage = "Too few animation positions set: '" + currentAnimationSetup.Count + "'!";
			Debug.LogError(errorMessage);
			MessageManager.Instance.RpcInformClients(errorMessage, MessageType.WARNING);
			return false;
		}
		if (animSettingsManager.animType == AnimationType.Key && currentAnimationSetup.Count != 2) {
			string errorMessage = "'Key' animation requires '2' positions set!";
			Debug.LogError(errorMessage);
			MessageManager.Instance.RpcInformClients(errorMessage, MessageType.WARNING);
			return false;
		}
		return true;
	}

	/// <summary>
	/// Stops animation listening and ends all animations
	/// </summary>
	[Server]
	public void stopTraining() {
		if (!isTrainingRunning) {
			return;
		}
		Debug.Log("Listening to animation events - STOPPED");
		isTrainingRunning = false;
		currentRepetitions = 0;

		RpcStopTraining();
	}

	[Server]
	public void cancelTrainingOnServer() {
		if (!isTrainingRunning) {
			return;
		}
		Debug.Log("Listening to animation events - CANCELED");
		isTrainingRunning = false;
		currentRepetitions = 0;
	}

	[ClientRpc]
	public void RpcStartActualAnimation(bool isShowcase, string extraText) {
		if (CharacterManager.activePatientInstance == null) {
			MessageManager.Instance.showMessage("No active patient", MessageType.WARNING);
			return;
		}
		bool animationResult = CharacterManager.activePatientInstance.activeArmAnimationController.startAnimation(isShowcase);

		if (!isShowcase && !animationResult) {
			clientStopTraining();

			if (SettingsManager.Instance.roleSettings.characterRole == UserRole.Patient) {
				NetworkCharacterManager.localNetworkClientInstance.CmdCancelTraining();
			}
		}

		NetworkCharacterManager.localNetworkClientInstance.pauseCountdown(extraText);
	}

	[ClientRpc]
	public void RpcStopActualAnimation() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}

		CharacterManager.activePatientInstance.activeArmAnimationController.stopAnimation();
	}

	[ClientRpc]
	public void RpcStartTraining(float duration, string extraText) {
		NetworkCharacterManager.localNetworkClientInstance.startCountdown(duration, extraText);

		NetworkCharacterManager.localNetworkClientInstance.trainingStarted();

		MessageManager.Instance.showMessage("Training started.", MessageType.OK);
	}

	[ClientRpc]
	public void RpcStartCountdown(float duration, string extraText) {
		NetworkCharacterManager.localNetworkClientInstance.startCountdown(duration, extraText);
	}

	[ClientRpc]
	public void RpcStopTraining(string message, MessageType messageType) {
		clientStopTraining(message, messageType);
	}

	[ClientRpc]
	public void RpcStopTraining() {
		clientStopTraining("Training stopped.", MessageType.NORMAL);
	}

	[Client]
	private void clientStopTraining() {
		NetworkCharacterManager.localNetworkClientInstance.stopCountdown();

		NetworkCharacterManager.localNetworkClientInstance.trainingStopped();
	}

	[Client]
	private void clientStopTraining(string message, MessageType messageType) {
		clientStopTraining();

		MessageManager.Instance.showMessage(message, messageType);
	}
}
