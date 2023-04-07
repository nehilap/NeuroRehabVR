using System.Collections.Generic;
using Enums;
using NeuroRehab.Mappings;
using Mirror;
using UnityEngine;
using NeuroRehab.Utility;
using Unity.XR.CoreUtils;

public class NetworkCharacterManager : NetworkBehaviour {

	public static NetworkCharacterManager localNetworkClientInstance { get; private set; }

	[SerializeField] private AnimationSettingsManager animSettingsManager;

	[SerializeField] private List<GameObject> targetPrefabs = new List<GameObject>();

	[SerializeField] private GameObject spawnArea;

	[SerializeField] private List<CountdownManager> countdownManagers = new List<CountdownManager>();

	private Transform _mirror;

	void Start() {
		spawnArea = ObjectManager.Instance.getFirstObjectByName("SpawnArea");
		animSettingsManager = ObjectManager.Instance.getFirstObjectByName("AnimationSettingsManager")?.GetComponent<AnimationSettingsManager>();
		List<GameObject> countdownObjects = ObjectManager.Instance.getObjectsByName("Countdown");
		foreach (GameObject item in countdownObjects) {
			countdownManagers.Add(item.GetComponent<CountdownManager>());
		}

		if (spawnArea == null || animSettingsManager == null) {
			Debug.LogError("'AnimationSettingsManager' or 'SpawnArea' not found");
			return;
		}

		if (isLocalPlayer) {
			spawnCorrectTargetFakes(AnimationType.Off, animSettingsManager.animType);
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
	/// <param name="newPlayerOwner"></param>
	[Command]
	public void CmdSetItemAuthority(NetworkIdentity item, NetworkIdentity newPlayerOwner) {
		setItemAuthority(item, newPlayerOwner);
	}

	private void setItemAuthority(NetworkIdentity item, NetworkIdentity newPlayerOwner) {
		// No need to re-assign authority
		if (newPlayerOwner.connectionToClient.owned.Contains(item)) {
			return;
		}
		item.gameObject.GetComponent<NetworkTransform>().syncDirection = SyncDirection.ClientToServer;
		Debug.Log("Granting authority: '" + item.netId + "' to: '" + newPlayerOwner.netId + "'");
		item.RemoveClientAuthority();
		item.AssignClientAuthority(newPlayerOwner.connectionToClient);

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
	public void CmdUpdateAnimType(AnimationType _animType) {
		if (animSettingsManager.animType == _animType) return;

		animSettingsManager.animType = _animType;
	}

	/*
	*
	* SYNCING START / END POSITIONS
	*
	*/

	[Command]
	public void CmdSetAnimationStartPosition() {
		setAnimationStartPosition();
	}

	/// <summary>
	/// Changes or Adds new start position
	/// </summary>
	private void setAnimationStartPosition() {
		if (!isServer) {
			return;
		}

		PosRotMapping targetPosRotMapping = getPosRotFromCurrentObject();
		if (targetPosRotMapping == null) {
			return;
		}

		if (!isTargetInBounds(targetPosRotMapping)) {
			Debug.LogWarning("Cannot set target position - Out of range of Arm");
			return;
		}

		// due to how SyncList works we can't simply change value in list element, we have to replace whole element
		// https://mirror-networking.gitbook.io/docs/manual/guides/synchronization/synclists
		if (animSettingsManager.getCurrentAnimationSetup().Count >= 1) {
			animSettingsManager.getCurrentAnimationSetup()[0] = targetPosRotMapping;
		} else {
			animSettingsManager.getCurrentAnimationSetup().Add(targetPosRotMapping);
		}
	}

	private void setAnimationStartPosition(PosRotMapping targetPosRotMapping) {
		if (!isServer) {
			return;
		}

		if (targetPosRotMapping == null) {
			return;
		}

		if (!isTargetInBounds(targetPosRotMapping)) {
			Debug.LogWarning("Cannot set target position - Out of range of Arm");
			return;
		}

		// due to how SyncList works we can't simply change value in list element, we have to replace whole element
		// https://mirror-networking.gitbook.io/docs/manual/guides/synchronization/synclists
		if (animSettingsManager.getCurrentAnimationSetup().Count >= 1) {
			animSettingsManager.getCurrentAnimationSetup()[0] = targetPosRotMapping;
		} else {
			animSettingsManager.getCurrentAnimationSetup().Add(targetPosRotMapping);
		}
	}

	/// <summary>
	/// Helper method for getting posRot of current target object
	/// </summary>
	/// <returns></returns>
	private PosRotMapping getPosRotFromCurrentObject() {
		GameObject targetObject = ObjectManager.Instance.getFirstObjectByName(animSettingsManager.animType.ToString());

		if (targetObject == null) {
			Debug.LogError("Failed to find object: " + animSettingsManager.animType.ToString());
			return null;
		}

		return new PosRotMapping(targetObject.transform);
	}

	/// <summary>
	/// Adds new move position at the end of synclist
	/// /// </summary>
	[Command]
	public void CmdAddMovePosition() {
		if (animSettingsManager.getCurrentAnimationSetup().Count >= 10) {
			return;
		}

		PosRotMapping movePosRotMapping = getPosRotFromCurrentObject();
		if (movePosRotMapping == null) {
			return;
		}
		if (!isTargetInBounds(movePosRotMapping)) {
			Debug.LogWarning("Cannot set target position - Out of range of Arm");
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
		setLockPosition(lockTargetPosRot);
	}

	/// <summary>
	/// Sets the lock position at the end of synclist
	/// </summary>
	/// <param name="lockTargetPosRot"></param>
	private void setLockPosition(PosRotMapping lockTargetPosRot) {
		if (!isServer) {
			return;
		}

		if (animSettingsManager.animType != AnimationType.Key) {
			return;
		}

		if (!isTargetInBounds(lockTargetPosRot)) {
			Debug.LogWarning("Cannot set Lock position - Out of range of Arm");
			return;
		}

		// we only allow 2 positions in case of Key animation
		int setupCount = animSettingsManager.getCurrentAnimationSetup().Count;
		if (setupCount > 1) {
			animSettingsManager.getCurrentAnimationSetup()[setupCount - 1] = lockTargetPosRot;
		} else {
			animSettingsManager.getCurrentAnimationSetup().Add(lockTargetPosRot);
		}
	}

	/// <summary>
	/// Checks if object is above table AND in range of arm. If there is no active patient only checks if object is above table
	/// </summary>
	/// <param name="targetPosRotMapping"></param>
	/// <returns></returns>
	public bool isTargetInBounds(PosRotMapping targetPosRotMapping) {
		GameObject tableObject = ObjectManager.Instance.getFirstObjectByName("Table");

		if (tableObject == null) {
			return false;
		}

		RaycastHit[] hits = Physics.RaycastAll(targetPosRotMapping.position, Vector3.down, targetPosRotMapping.position.y);
		bool isAboveTable = false;
		foreach (RaycastHit hit in hits) {
			if (hit.collider.gameObject.Equals(tableObject)) {
				isAboveTable = true;
				break;
			}
		}
		if (!isAboveTable) {
			Debug.LogWarning("Target Object not above Table");
			return false;
		}

		if (CharacterManager.activePatientInstance != null) {
			if (!CharacterManager.activePatientInstance.activeArmAnimationController.isTargetInRange(targetPosRotMapping.position)) {
				float armLength = CharacterManager.activePatientInstance.activeArmAnimationController.getArmLength() + CharacterManager.activePatientInstance.activeArmAnimationController.getArmRangeSlack();
				float targetDistance = Vector3.Distance(targetPosRotMapping.position, CharacterManager.activePatientInstance.activeArmAnimationController.getArmRangePosition());

				Debug.LogWarning("Arm cannot reach object, too far away: " + targetDistance + "m > " + armLength + "m");
				return false;
			}
		}

		return true;
	}

	[Command]
	public void CmdClearAnimationMovePositions() {
		animSettingsManager.getCurrentAnimationSetup().Clear();
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
		Debug.Log("Spawning object: '" + _newAnimType + "', old object: '" + _oldAnimType + "'");
		if (_newAnimType == _oldAnimType) {
			Debug.Log("Animation types equal, cancelling!");
			return;
		}

		spawnCorrectTarget(_oldAnimType, _newAnimType);

		RpcSpawnCorrectTarget(_oldAnimType, _newAnimType);
	}

	[ClientRpc]
	public void RpcSpawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType) {
		spawnCorrectTargetFakes(_oldAnimType, _newAnimType);
	}

	/// <summary>
	/// Method called on server, objects are then spawned on all clients. Refer to https://mirror-networking.gitbook.io/docs/guides/gameobjects/spawning-gameobjects for more details.
	/// </summary>
	/// <param name="_oldAnimType"></param>
	/// <param name="_newAnimType"></param>
	private void spawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType) {
		for (int i = 0; i < targetPrefabs.Count; i++) {
			if (targetPrefabs[i].name.Equals(_newAnimType.ToString())) {
				float halfHeight = targetPrefabs[i].GetComponent<Renderer>().bounds.extents.y;
				Vector3 rotation = targetPrefabs[i].transform.rotation.eulerAngles;

				// if animation is Key AND patient is left handed, we flip the key
				if (CharacterManager.activePatientInstance != null && _newAnimType == AnimationType.Key && CharacterManager.activePatientInstance.isLeftArmAnimated) {
					rotation = new Vector3(-90f, 0f, -90f); // did not find effective algorithm to mirror the key, so it is what it is
				}
				GameObject newObject = Instantiate(targetPrefabs[i], spawnArea.transform.position + new Vector3(0, halfHeight, 0), Quaternion.Euler(rotation));
				newObject.gameObject.name = targetPrefabs[i].name;

				NetworkServer.Spawn(newObject);
				setAnimationStartPosition();
			}

			// we spawn three locks in this case
			if (_newAnimType == AnimationType.Key) {
				if (targetPrefabs[i].name.Equals("Lock")) {
					float halfHeight = targetPrefabs[i].transform.lossyScale.y * targetPrefabs[i].GetComponent<MeshFilter>().sharedMesh.bounds.extents.y;

					// Center
					GameObject newLock1 = Instantiate(targetPrefabs[i], spawnArea.transform.position + new Vector3(0, halfHeight, 0.25f), targetPrefabs[i].transform.rotation);
					NetworkServer.Spawn(newLock1);
					// Left
					GameObject newLock2 = Instantiate(targetPrefabs[i], spawnArea.transform.position + new Vector3(-0.3f, halfHeight, 0.18f), targetPrefabs[i].transform.rotation);
					newLock2.transform.LookAt(spawnArea.transform.position + new Vector3(0, halfHeight, 0));
					NetworkServer.Spawn(newLock2);
					// Right
					GameObject newLock3 = Instantiate(targetPrefabs[i], spawnArea.transform.position + new Vector3(+0.3f, halfHeight, 0.18f), targetPrefabs[i].transform.rotation);
					newLock3.transform.LookAt(spawnArea.transform.position + new Vector3(0, halfHeight, 0));
					NetworkServer.Spawn(newLock3);

					setLockPosition(new PosRotMapping(newLock1.GetComponent<TargetUtility>().customTargetPos.transform));
				}
			}
		}

		// destroy old objects
		List<GameObject> oldTargetsInScene = ObjectManager.Instance.getObjectsByName(_oldAnimType.ToString());
		foreach (var item in oldTargetsInScene) {
			NetworkServer.Destroy(item);
		}
		if (_oldAnimType == AnimationType.Key) {
			oldTargetsInScene = ObjectManager.Instance.getObjectsByName("Lock");
			foreach (var item in oldTargetsInScene) {
				NetworkServer.Destroy(item);
			}
		}
	}

	private void spawnCorrectTargetFakes(AnimationType _oldAnimType, AnimationType _newAnimType) {
		for (int i = 0; i < targetPrefabs.Count; i++) {
			if (targetPrefabs[i].name.Equals(_newAnimType.ToString() + "_fake")) {
				Vector3 rotation = targetPrefabs[i].transform.rotation.eulerAngles;

				// if animation is Key AND patient is left handed, we flip the key
				if (CharacterManager.activePatientInstance != null && _newAnimType == AnimationType.Key && CharacterManager.activePatientInstance.isLeftArmAnimated) {
					rotation = new Vector3(-90f, 0f, -90f); // did not find effective algorithm to mirror the key, so it is what it is
				}
				GameObject newObject = Instantiate(targetPrefabs[i], spawnArea.transform.position, Quaternion.Euler(rotation));
				newObject.gameObject.name = targetPrefabs[i].name;
			}
		}

		// We de-activate fake here on client
		List<GameObject> oldTargetsInScene = ObjectManager.Instance.getObjectsByName(_oldAnimType.ToString() + "_fake");
		foreach (var item in oldTargetsInScene) {
			Destroy(item);
		}

		// We de-activate fake here on client
		oldTargetsInScene = ObjectManager.Instance.getObjectsByName(_oldAnimType.ToString());
		foreach (var item in oldTargetsInScene) {
			if (!item.activeSelf) {
				Destroy(item);
			}
		}
	}

	/**
	*
	* CALLING ANIMATIONS ON CLIENTS
	* server calls starts animation on every client separately, clients decide what to do with animation
	*
	*/

	[Command]
	public void CmdStartAnimationShowcase() {
		AnimationServerManager.Instance.RpcStartActualAnimation(true);
	}

	[Command]
	public void CmdStartAnimation() {
		AnimationServerManager.Instance.startTraining();
	}

	[Command]
	public void CmdStopAnimation() {
		AnimationServerManager.Instance.stopTraining();
	}

	[Command]
	public void CmdProgressAnimationStep() {
		AnimationServerManager.Instance.progressAnimationStep();
	}

	// alternates arm resting state SyncVar, which calls hook on clients and causes other events
	[Command]
	public void CmdSetArmRestPosition(NetworkIdentity patientIdentity) {
		patientIdentity.gameObject.GetComponent<CharacterManager>().isArmResting = !patientIdentity.gameObject.GetComponent<CharacterManager>().isArmResting;
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
	/// <param name="caller"></param>
	[Command]
	public void CmdMoveTable(Vector3 offset, NetworkIdentity caller) {
		List<GameObject> tableObjs =  ObjectManager.Instance.getObjectsByName("Table");
		if (tableObjs.Count == 0) {
			return;
		}
		foreach (var item in tableObjs) {
			item.transform.position += offset;
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
			setItemAuthority(item.GetComponent<NetworkIdentity>(), caller);
		}

		if (animSettingsManager.animType == AnimationType.Key) {
			List<GameObject> lockObjects = ObjectManager.Instance.getObjectsByName("Lock");
			foreach (var item in lockObjects) {
				setItemAuthority(item.GetComponent<NetworkIdentity>(), caller);
			}
		}

		// To keep sync direction consistent, we move target objects on caller Client
		TargetMoveObjects(caller.connectionToClient, offset);
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

			for (int i = 0; i < targetPrefabs.Count; i++) {
				if (targetPrefabs[i].name.Equals(animSettingsManager.animType.ToString())) {
					rotation = targetPrefabs[i].transform.rotation.eulerAngles;
					break;
				}
			}
			newMapping = new PosRotMapping(targetObject.transform.position, rotation);
			TargetTransformObject(patientId.connectionToClient, newMapping, targetObject.GetComponent<NetworkIdentity>(), true);
		}

		setAnimationStartPosition(newMapping);
	}

	[TargetRpc]
	public void TargetTransformObject(NetworkConnection connection, PosRotMapping mapping, NetworkIdentity itemId, bool resetStartPost) {
		if (!itemId.isOwned) {
			NetworkCharacterManager.localNetworkClientInstance.CmdSetItemAuthority(itemId, CharacterManager.localClientInstance.GetComponent<NetworkIdentity>());
		}

		itemId.gameObject.transform.position = mapping.position;
		itemId.gameObject.transform.rotation = Quaternion.Euler(mapping.rotation);
	}

	/**
	*
	* COUNTDOWN
	*
	*/

	public void startCountdown() {
		foreach (CountdownManager countdownManager in countdownManagers) {
			countdownManager.startCountdown(animSettingsManager.waitDuration);
		}
	}

	public void pauseCountdown() {
		foreach (CountdownManager countdownManager in countdownManagers) {
			countdownManager.stopCountdown();
		}
	}

	public void stopCountdown() {
		foreach (CountdownManager countdownManager in countdownManagers) {
			countdownManager.stopCountdown();
			countdownManager.hideCountdown();
		}
	}
}
