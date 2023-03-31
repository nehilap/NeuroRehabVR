using UnityEngine;
using System;
using Mirror;
using ShadowGroveGames.SimpleHttpAndRestServer.Scripts;

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

	private bool isAnimationRunning = false;
	private bool isAnimationTriggered = false;
	private DateTime lastAnimationTrigger;
	private int currentRepetitions = 0;

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
/*
	void Update() {
		if (isAnimationRunning) {
			DateTime currentTime = DateTime.Now;
			if ((currentTime - lastAnimationTrigger).TotalSeconds > animSettingsManager.waitDuration) {
				isAnimationRunning = false;
			}
		}
	}
 */

	/// <summary>
	/// Starts animation if it's in time and we still have repetitions left
	/// </summary>
	/// <returns>Whether move was succesfully triggered</returns>
	public bool moveArm() {
		//Debug.Log("Move arm called");
		if (!isAnimationTriggered && isAnimationRunning) {
			DateTime currentTime = DateTime.Now;
			Debug.Log("Seconds since last animation step: " + (currentTime - lastAnimationTrigger).TotalSeconds);
			if ((currentTime - lastAnimationTrigger).TotalSeconds <= animSettingsManager.waitDuration) {
				Debug.Log("Starting arm animation");
				RpcStartActualAnimation(false);
				isAnimationTriggered = true;
				return true;
			} else {
				isAnimationRunning = false;
				isAnimationTriggered = false;
			}
		}
		return false;
	}

	/// <summary>
	/// Marks animation step as done
	/// </summary>
	public void progressAnimationStep() {
		if (!isAnimationRunning) {
			return;
		}

		// Debug.Log("animation step received");
		DateTime currentTime = DateTime.Now;
		lastAnimationTrigger = currentTime;
		currentRepetitions++;
		if (currentRepetitions >= animSettingsManager.repetitions) {
			isAnimationRunning = false;
		}
		isAnimationTriggered = false;
	}

	/// <summary>
	/// Starts listening to animation move events
	/// </summary>
	/// <returns>Whether we succesfully started listening to new moves</returns>
	public bool startAnimationListening() {
		if (isAnimationRunning) {
			DateTime currentTime = DateTime.Now;
			if ((currentTime - lastAnimationTrigger).TotalSeconds <= animSettingsManager.waitDuration) {
				return false;
			}
		}

		Debug.Log("Listening to animation events - STARTED");
		isAnimationRunning = true;
		isAnimationTriggered = false;
		lastAnimationTrigger = DateTime.Now;
		currentRepetitions = 0;

		return true;
	}

	/// <summary>
	/// Stops animation listening and ends all animations
	/// </summary>
	public void stopTraining() {
		Debug.Log("Listening to animation events - STOPPED");
		isAnimationRunning = false;
		currentRepetitions = 0;

		// RpcStopActualAnimation();
	}

	[ClientRpc]
	public void RpcStartActualAnimation(bool isShowcase) {
		// Debug.Log(CharacterManager.activePatientInstance);
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		CharacterManager.activePatientInstance.activeArmAnimationController.startAnimation(isShowcase);
	}

	[ClientRpc]
	public void RpcStopActualAnimation() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}

		CharacterManager.activePatientInstance.activeArmAnimationController.stopAnimation();
	}
}
