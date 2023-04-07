using UnityEngine;
using System;
using Mirror;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts;
using System.Collections;

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

	private bool isTrainingRunning = false;
	private bool isAnimationTriggered = false;
	private DateTime lastAnimationTrigger;
	private int currentRepetitions = 0;
	private Coroutine trainingCoroutine;

	private void Start() {
		if (!isServer) {
			return;
		}

		animSettingsManager = ObjectManager.Instance.getFirstObjectByName("AnimationSettingsManager")?.GetComponent<AnimationSettingsManager>();

		if (gameObject.TryGetComponent<SimpleEventServerScript>(out SimpleEventServerScript serverScript)) {
			if (isServer) {
				serverScript.enabled = true;
			}
		}
	}

	private IEnumerator waitDurationCoroutine(float duration) {
		yield return new WaitForSeconds(duration);
		if (!isTrainingRunning) {
			yield break;
		}
		isTrainingRunning = false;
		isAnimationTriggered = false;

		Debug.Log("Listening to animation events - Canceled");
		yield return new WaitForSeconds(2.5f);
		RpcStopTraining();
	}

	/// <summary>
	/// Starts animation if it's in time and we still have repetitions left
	/// </summary>
	/// <returns>Whether move was succesfully triggered</returns>
	public bool moveArm() {
		//Debug.Log("Move arm called");
		if (!isAnimationTriggered && isTrainingRunning) {
			DateTime currentTime = DateTime.Now;
			Debug.Log("Seconds since last animation step: " + (currentTime - lastAnimationTrigger).TotalSeconds);
			if ((currentTime - lastAnimationTrigger).TotalSeconds <= animSettingsManager.waitDuration) {
				Debug.Log("Starting arm animation");
				RpcStartActualAnimation(false);
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
			RpcStartCountdown();
		}

		trainingCoroutine = StartCoroutine(waitDurationCoroutine(animSettingsManager.waitDuration));

		isAnimationTriggered = false;
	}

	/// <summary>
	/// Starts listening to animation move events
	/// </summary>
	/// <returns>Whether we succesfully started listening to new moves</returns>
	public bool startTraining() {
		if (isTrainingRunning) {
			DateTime currentTime = DateTime.Now;
			if ((currentTime - lastAnimationTrigger).TotalSeconds <= animSettingsManager.waitDuration) {
				return false;
			}
		}

		Debug.Log("Listening to animation events - STARTED");
		isTrainingRunning = true;
		isAnimationTriggered = false;
		lastAnimationTrigger = DateTime.Now;
		currentRepetitions = 0;

		RpcStartTraining();
		if (trainingCoroutine != null) {
			StopCoroutine(trainingCoroutine);
		}
		trainingCoroutine = StartCoroutine(waitDurationCoroutine(animSettingsManager.waitDuration));

		return true;
	}

	/// <summary>
	/// Stops animation listening and ends all animations
	/// </summary>
	public void stopTraining() {
		if (!isTrainingRunning) {
			return;
		}
		Debug.Log("Listening to animation events - STOPPED");
		isTrainingRunning = false;
		currentRepetitions = 0;

		// RpcStopActualAnimation();
		RpcStopTraining();
	}

	[ClientRpc]
	public void RpcStartActualAnimation(bool isShowcase) {
		// Debug.Log(CharacterManager.activePatientInstance);
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		CharacterManager.activePatientInstance.activeArmAnimationController.startAnimation(isShowcase);

		NetworkCharacterManager.localNetworkClientInstance.pauseCountdown();
	}

	[ClientRpc]
	public void RpcStopActualAnimation() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}

		CharacterManager.activePatientInstance.activeArmAnimationController.stopAnimation();
	}

	[ClientRpc]
	public void RpcStartTraining() {
		NetworkCharacterManager.localNetworkClientInstance.startCountdown();

		NetworkCharacterManager.localNetworkClientInstance.trainingStarted();
	}

	[ClientRpc]
	public void RpcStartCountdown() {
		NetworkCharacterManager.localNetworkClientInstance.startCountdown();
	}

	[ClientRpc]
	public void RpcStopTraining() {
		NetworkCharacterManager.localNetworkClientInstance.stopCountdown();

		NetworkCharacterManager.localNetworkClientInstance.trainingStopped();
	}
}
