using System.Collections.Generic;
using NeuroRehab.Enums;
using NeuroRehab.Mappings;
using Mirror;
using UnityEngine;
using Unity.XR.CoreUtils;

public class NetworkCharacterManager : NetworkBehaviour {

	public static NetworkCharacterManager localNetworkClientInstance { get; private set; }

	[SerializeField] private AnimationSettingsManager animSettingsManager;

	[SerializeField] private List<CountdownManager> countdownManagers = new List<CountdownManager>();
	[SerializeField] private List<TherapistMenuManager> therapistMenuManagers = new List<TherapistMenuManager>();

	private Transform _mirror;

	void Start() {
		animSettingsManager = ObjectManager.Instance.getFirstObjectByName("AnimationSettingsManager")?.GetComponent<AnimationSettingsManager>();
		List<GameObject> countdownObjects = ObjectManager.Instance.getObjectsByName("Countdown");
		foreach (GameObject item in countdownObjects) {
			if (item.TryGetComponent<CountdownManager>(out CountdownManager cm)) {
				countdownManagers.Add(cm);
			}
		}

		List<GameObject> therapistMenuObjects = ObjectManager.Instance.getObjectsByName("TherapistMenu");
		foreach (GameObject therapistMenuObject in therapistMenuObjects) {
			if (therapistMenuObject.TryGetComponent<TherapistMenuManager>(out TherapistMenuManager tmm)) {
				therapistMenuManagers.Add(tmm);
			}
		}

		if (animSettingsManager == null) {
			Debug.LogError("'AnimationSettingsManager' not found");
			return;
		}

		if (isLocalPlayer) {
			animSettingsManager.spawnCorrectTargetFakes(AnimationType.Off, animSettingsManager.animType);
		}

		_mirror = ObjectManager.Instance.getFirstObjectByName("MirrorPlane")?.transform;
		if (_mirror == null) {
			Debug.LogError("Failed to initialize ArmAnimationController - 'MirrorPlane' not found");
			return;
		}
	}

	public override void OnStartClient () {
		base.OnStartClient();

		if (isLocalPlayer) {
			localNetworkClientInstance = this;
		}
	}

	/*
	*
	* ITEM PICKUP
	*
	*/

	/// <summary>
	/// Command wrapper to change item authority
	/// </summary>
	/// <param name="item"></param>
	/// <param name="sender"></param>
	[Command]
	public void CmdSetItemAuthority(NetworkIdentity item, NetworkConnectionToClient sender = null) {
		setItemAuthority(item, sender);
	}

	[Server]
	private void setItemAuthority(NetworkIdentity item, NetworkConnectionToClient sender = null) {
		// No need to re-assign authority
		if (sender.owned.Contains(item)) {
			return;
		}
		item.gameObject.GetComponent<NetworkTransform>().syncDirection = SyncDirection.ClientToServer;
		Debug.Log($"Granting authority: OBJECT '{item.netId}' to: USER '{sender.identity.netId}'");
		item.RemoveClientAuthority();
		item.AssignClientAuthority(sender);

		// force reset snapshots, new client does not have the correct knowledge of previous snapshots
		item.gameObject.GetComponent<NetworkTransform>().Reset();
	}

	/*
	* COMMANDS FOR SERVER - SYNCVARs
	*
	*/

	[Command]
	public void CmdUpdateArmDuration(float value) {
		if (animSettingsManager.armMoveDuration == value) return;

		animSettingsManager.armMoveDuration = value;
	}

	[Command]
	public void CmdUpdateHandDuration(float value) {
		if (animSettingsManager.handMoveDuration == value) return;

		animSettingsManager.handMoveDuration = value;
	}

	[Command]
	public void CmdUpdateWaitDuration(float value) {
		if (animSettingsManager.waitDuration == value) return;

		animSettingsManager.waitDuration = value;
	}

	[Command]
	public void CmdUpdateMoveDuration(float value) {
		if (animSettingsManager.moveDuration == value) return;

		animSettingsManager.moveDuration = value;
	}

	[Command]
	public void CmdUpdateRepetitions(int value) {
		if (animSettingsManager.repetitions == value) return;

		animSettingsManager.repetitions = value;
	}

	[Command]
	public void CmdUpdateAnimType(AnimationType _oldAnimType, AnimationType _newAnimType, NetworkConnectionToClient sender = null) {
		if (animSettingsManager.animType == _newAnimType) return;

		if (animSettingsManager.setAnimType(_oldAnimType, _newAnimType)) {
			animSettingsManager.spawnCorrectTarget(_oldAnimType, _newAnimType);
			animSettingsManager.RpcSpawnCorrectTarget(_oldAnimType, _newAnimType);
		} else {
			TargetSetAnimTypeDropdownValue(sender, _oldAnimType);
			MessageManager.Instance.RpcInformClients("Unable to change animation type at this moment!", MessageType.NORMAL);
		}
	}

	[TargetRpc]
	public void TargetSetAnimTypeDropdownValue(NetworkConnection connection, AnimationType animationType) {
		animSettingsManager.setAnimTypeDropdownText(animationType.ToString());
	}

	/*
	*
	* SYNCING START / END POSITIONS
	*
	*/

	[Command]
	public void CmdSetAnimationStartPosition() {
		animSettingsManager.setAnimationStartPosition(true);
	}

	/// <summary>
	/// Adds new move position at the end of synclist. Currently server assigns the value. In case there is an issue with correct position (server jitter), we could send the value over network from client.
	/// </summary>
	[Command]
	public void CmdAddMovePosition() {
		if (animSettingsManager.getCurrentAnimationSetup().Count >= 10) {
			return;
		}

		PosRotMapping movePosRotMapping = animSettingsManager.getPosRotFromCurrentObject();
		if (movePosRotMapping == null) {
			return;
		}
		if (!animSettingsManager.isTargetInBounds(movePosRotMapping)) {
			Debug.LogWarning("Cannot set target position - 'target object' not in bounds");
			return;
		}
		animSettingsManager.getCurrentAnimationSetup().Add(movePosRotMapping);
	}

	/// <summary>
	/// Command wrapper for setting lock position
	/// </summary>
	/// <param name="lockTargetPosRot"></param>
	[Command]
	public void CmdSetLockPosition(PosRotMapping lockTargetPosRot) {
		animSettingsManager.setLockPosition(lockTargetPosRot);
	}


	[Command]
	public void CmdClearMovePositions() {
		animSettingsManager.getCurrentAnimationSetup().Clear();
	}

	/// <summary>
	/// Deletes last move position
	/// </summary>
	[Command]
	public void CmdDeleteMovePosition() {
		int lastIndex = animSettingsManager.getCurrentAnimationSetup().Count - 1;
		if (lastIndex < 0 || !(animSettingsManager.getCurrentAnimationSetup().Count > lastIndex)) {
			return;
		}
		animSettingsManager.getCurrentAnimationSetup().RemoveAt(animSettingsManager.getCurrentAnimationSetup().Count - 1);
	}

	/**
	*
	* TARGET OBJECT SPAWNING
	*
	*/

	/// <summary>
	/// Command wrapper to spawn correct target object
	/// </summary>
	/// <param name="_oldAnimType"></param>
	/// <param name="_newAnimType"></param>
	[Command]
	public void CmdSpawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType) {
		if (_newAnimType == _oldAnimType) {
			Debug.Log($"Animation types equal '{_newAnimType}'. Cancelling spawn!");
			return;
		}

		animSettingsManager.spawnCorrectTarget(_oldAnimType, _newAnimType);
		animSettingsManager.RpcSpawnCorrectTarget(_oldAnimType, _newAnimType);
	}

	/**
	*
	* CALLING ANIMATIONS ON CLIENTS
	* server calls starts animation on every client separately, clients decide what to do with animation
	*
	*/

	[Command]
	public void CmdStartAnimationShowcase() {
		AnimationServerManager.Instance.RpcStartActualAnimation(true, "");
	}

	[Command]
	public void CmdStartAnimation() {
		AnimationServerManager.Instance.RpcStartActualAnimation(false, "");
	}

	[Command]
	public void CmdStartTraining() {
		AnimationServerManager.Instance.startTraining();
	}

	[Command]
	public void CmdStopTraining() {
		AnimationServerManager.Instance.stopTraining();
	}

	[Command]
	public void CmdCancelTraining() {
		AnimationServerManager.Instance.cancelTrainingOnServer();
	}

	[Command]
	public void CmdProgressAnimationStep() {
		AnimationServerManager.Instance.progressAnimationStep();
	}

	/// <summary>
	/// Alternates arm resting state SyncVar, which calls hook on clients and causes other events
	/// </summary>
	/// <param name="patientIdentity"></param>
	[Command]
	public void CmdSetArmRestPosition(NetworkIdentity patientIdentity) {
		patientIdentity.gameObject.GetComponent<CharacterManager>().isArmResting = !patientIdentity.gameObject.GetComponent<CharacterManager>().isArmResting;
	}

	[Command]
	public void CmdSetAnimationState(NeuroRehab.Enums.AnimationState animationState, NetworkConnectionToClient sender = null) {
		sender.identity.gameObject.GetComponent<CharacterManager>().activeArmAnimationController.setAnimState(animationState);
	}

	/*
	*
	* Setting up positioning
	*
	*/

	[Command]
	public void CmdMovePatientToSit(NetworkIdentity patientIdentity) {
		if (patientIdentity == null) {
			return;
		}

		TargetMovePatientToSit(patientIdentity.connectionToClient);
	}

	/// <summary>
	/// Method to teleport patient behind table AND rotate them to look in the direction 'PatientSitPositionObject' is rotated
	/// </summary>
	/// <param name="connection"></param>
	[TargetRpc]
	public void TargetMovePatientToSit(NetworkConnection connection) {
		GameObject patientSitPosition = ObjectManager.Instance.getFirstObjectByName("PatientSitPositionObject");
		GameObject tableObject = ObjectManager.Instance.getFirstObjectByName("Table");
		// Table here is mostly ignored since we don't expect Parient to ever use DesktopCharacterManager

		CharacterManager.localClientInstance.teleportCharacter(patientSitPosition.transform, tableObject.transform);
	}

	/// <summary>
	/// Method to move table up or down. On top of moving table we also have to change ALL markers and target object as well
	/// </summary>
	/// <param name="offset"></param>
	/// <param name="sender"></param>
	[Command]
	public void CmdMoveTable(Vector3 offset, NetworkConnectionToClient sender = null) {
		List<GameObject> tableObjs = ObjectManager.Instance.getObjectsByName("Table");
		if (tableObjs.Count == 0) {
			return;
		}
		int tableObjsMoved = 0;
		foreach (var tableObj in tableObjs) {
			if ((tableObj.transform.position.y + offset.y) <= -0.5f || (tableObj.transform.position.y + offset.y) > 0.8f) {
				continue;
			}
			tableObj.transform.position += offset;
			tableObjsMoved++;
		}

		if (tableObjsMoved == 0) {
			return;
		}
		// we have to completely change object holding position, otherwise it won't be synced to clients
		// Refer to https://mirror-networking.gitbook.io/docs/manual/guides/synchronization/synclists for more details
		List<SyncList<PosRotMapping>> allSetups = animSettingsManager.getAllAnimationSetups();

		foreach (SyncList<PosRotMapping> setup in allSetups) {
			for (int i = 0; i < setup.Count; i++) {
				setup[i] = new PosRotMapping(setup[i].position + offset, setup[i].rotation);
			}
		}

		List<GameObject> targetObjects = ObjectManager.Instance.getObjectsByName(animSettingsManager.animType.ToString());
		if (targetObjects.Count == 0) {
			return;
		}
		foreach (var item in targetObjects) {
			setItemAuthority(item.GetComponent<NetworkIdentity>(), sender);
		}

		if (animSettingsManager.animType == AnimationType.Key) {
			List<GameObject> lockObjects = ObjectManager.Instance.getObjectsByName("Lock");
			foreach (var item in lockObjects) {
				setItemAuthority(item.GetComponent<NetworkIdentity>(), sender);
			}
		}

		// To keep sync direction consistent, we move target objects on caller Client
		TargetMoveObjects(sender, offset);
	}

	[Command]
	public void CmdMoveArmRest(Vector3 offset) {
		GameObject rightArmRest = ObjectManager.Instance.getFirstObjectByName("ArmRestHelperObjectRight");
		GameObject leftArmRest = ObjectManager.Instance.getFirstObjectByName("ArmRestHelperObjectLeft");

		if (rightArmRest) {
			rightArmRest.transform.position += offset;
		}
		if (leftArmRest) {
			leftArmRest.transform.position += offset;
		}
	}

	/// <summary>
	/// Moving target object(s) on client
	/// </summary>
	/// <param name="connection"></param>
	/// <param name="offset"></param>
	[TargetRpc]
	public void TargetMoveObjects(NetworkConnection connection, Vector3 offset) {
		string targetObjectName = animSettingsManager.animType.ToString();

		List<GameObject> targetObjects = ObjectManager.Instance.getObjectsByName(targetObjectName);
		foreach (var item in targetObjects) {
			item.transform.position += offset;
		}

		if (animSettingsManager.animType == AnimationType.Key) {
			List<GameObject> lockObjects = ObjectManager.Instance.getObjectsByName("Lock");
			foreach (var item in lockObjects) {
				item.transform.position += offset;
			}
		}
	}

	/// <summary>
	/// Method to move patient object by offset
	/// </summary>
	/// <param name="offset"></param>
	/// <param name="patientId"></param>
	[Command]
	public void CmdMovePatient(Vector3 offset, NetworkIdentity patientId) {
		if (patientId == null) {
			return;
		}

		TargetMovePatient(patientId.connectionToClient, offset);
	}

	/// <summary>
	/// Method to move patient camera. Moved patient based on direction
	/// </summary>
	/// <param name="connection"></param>
	/// <param name="offset"></param>
	[TargetRpc]
	public void TargetMovePatient(NetworkConnection connection, Vector3 offset) {
		Vector3 sidewayMovement = CharacterManager.localClientInstance.cameraObject.transform.right * offset.x;
		Vector3 forwardMovement = CharacterManager.localClientInstance.cameraObject.transform.forward * offset.z;
		Vector3 movement = sidewayMovement + forwardMovement;
		movement.y = 0;

		if (CharacterManager.localClientInstance.TryGetComponent<XROrigin>(out XROrigin xrOrigin)) {
			xrOrigin.MoveCameraToWorldLocation(CharacterManager.localClientInstance.cameraObject.transform.position + movement);
		} else {
			CharacterManager.localClientInstance.transform.position += movement;
		}
	}

	/*
	*
	* Setting active arm
	*
	*/

	[Command]
	public void CmdSetActiveArm(bool isLeftAnimated, NetworkIdentity patientId) {
		patientId.gameObject.TryGetComponent<CharacterManager>(out CharacterManager characterManager);
		if (!characterManager) {
			return;
		}

		characterManager.isLeftArmAnimated = isLeftAnimated;
		characterManager.changeAnimatedArm(false, isLeftAnimated);

		if (animSettingsManager.animType != AnimationType.Key) {
			return;
		}
		GameObject targetObject = ObjectManager.Instance.getFirstObjectByName(animSettingsManager.animType.ToString());

		PosRotMapping newMapping;
		if (isLeftAnimated) {
			newMapping = new PosRotMapping(targetObject.transform.position, new Vector3(-90f, 0f, -90f));
			TargetTransformObject(patientId.connectionToClient, newMapping, targetObject.GetComponent<NetworkIdentity>(), true);
		} else {
			Vector3 rotation = targetObject.transform.rotation.eulerAngles;

			for (int i = 0; i < animSettingsManager.targetPrefabs.Count; i++) {
				if (animSettingsManager.targetPrefabs[i].name.Equals(animSettingsManager.animType.ToString())) {
					rotation = animSettingsManager.targetPrefabs[i].transform.rotation.eulerAngles;
					break;
				}
			}
			newMapping = new PosRotMapping(targetObject.transform.position, rotation);
			TargetTransformObject(patientId.connectionToClient, newMapping, targetObject.GetComponent<NetworkIdentity>(), true);
		}

		animSettingsManager.setAnimationStartPosition(true, newMapping);
	}

	[TargetRpc]
	public void TargetTransformObject(NetworkConnection connection, PosRotMapping mapping, NetworkIdentity itemId, bool resetStartPost) {
		if (!itemId.isOwned) {
			NetworkCharacterManager.localNetworkClientInstance.CmdSetItemAuthority(itemId);
		}

		itemId.gameObject.transform.position = mapping.position;
		itemId.gameObject.transform.rotation = Quaternion.Euler(mapping.rotation);
	}

	/**
	*
	* TRAINING + COUNTDOWN
	*
	*/

	public void startCountdown(float duration, string extraText) {
		foreach (CountdownManager countdownManager in countdownManagers) {
			countdownManager.startCountdown(duration, extraText);
		}
	}

	public void pauseCountdown(string extraText) {
		foreach (CountdownManager countdownManager in countdownManagers) {
			countdownManager.stopCountdown(extraText);
		}
	}

	public void stopCountdown() {
		foreach (CountdownManager countdownManager in countdownManagers) {
			countdownManager.stopCountdown();
			countdownManager.hideCountdown();
		}
	}

	public void trainingStarted() {
		foreach (TherapistMenuManager therapistMenuManager in therapistMenuManagers) {
			foreach (CanvasGroup cg in therapistMenuManager.animationCanvases) {
				cg.interactable = false;
				cg.blocksRaycasts = false;
			}
		}
	}

	public void trainingStopped() {
		foreach (TherapistMenuManager therapistMenuManager in therapistMenuManagers) {
			foreach (CanvasGroup cg in therapistMenuManager.animationCanvases) {
				cg.interactable = true;
				cg.blocksRaycasts = true;
			}
		}
	}
}
