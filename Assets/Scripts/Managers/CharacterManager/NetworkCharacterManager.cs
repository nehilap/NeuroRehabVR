using System.Collections.Generic;
using Enums;
using Mappings;
using Mirror;
using UnityEngine;
using Utility;

public class NetworkCharacterManager : NetworkBehaviour {

	public static NetworkCharacterManager localNetworkClientInstance { get; private set; }

	[SerializeField] private AnimationSettingsManager animSettingsManager;
	
	[SerializeField] private List<GameObject> targetPrefabs = new List<GameObject>();

	[SerializeField] private GameObject spawnArea;

	private Transform _mirror;

	void Start() {
		spawnArea = ObjectManager.Instance.getFirstObjectByName("SpawnArea");
		animSettingsManager = ObjectManager.Instance.getFirstObjectByName("AnimationSettingsManager")?.GetComponent<AnimationSettingsManager>();

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

	[Command]
	public void CmdSetItemAuthority(NetworkIdentity item, NetworkIdentity newPlayerOwner) {
		setItemAuthority(item, newPlayerOwner);
	}

	private void setItemAuthority(NetworkIdentity item, NetworkIdentity newPlayerOwner) {
		item.gameObject.GetComponent<NetworkTransform>().syncDirection = SyncDirection.ClientToServer;
		Debug.Log("Granting authority:" + item.netId + " to:" + newPlayerOwner.netId);
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
		SetAnimationStartPosition();
	}

	private void SetAnimationStartPosition() {
		if (!isServer) {
			return;
		}

		PosRotMapping targetPosRotMapping = getPosRotFromObject();
		if (targetPosRotMapping == null) {
			return;
		}

		if (!isTargetInBounds(targetPosRotMapping)) {
			Debug.LogWarning("Cannot set target position - Out of range of Arm");
			return;
		}

		if (animSettingsManager.getCurrentAnimationSetup().Count >= 1) {
			animSettingsManager.getCurrentAnimationSetup()[0] = targetPosRotMapping;
		} else {
			animSettingsManager.getCurrentAnimationSetup().Add(targetPosRotMapping);
		}
	}

	private PosRotMapping getPosRotFromObject() {
		GameObject targetObject = ObjectManager.Instance.getFirstObjectByName(animSettingsManager.animType.ToString());
		
		if (targetObject == null) {
			Debug.LogError("Failed to find object: " + animSettingsManager.animType.ToString());
			return null;
		}
		
		return new PosRotMapping(targetObject.transform);
	}

	[Command]
	public void CmdAddMovePosition() {
		GameObject targetObject = ObjectManager.Instance.getFirstObjectByName(animSettingsManager.animType.ToString());
		
		if (targetObject == null) {
			Debug.LogError("Failed to find object: " + animSettingsManager.animType.ToString());
			return;
		}
		PosRotMapping movePosRotMapping = new PosRotMapping(targetObject.transform);

		if (!isTargetInBounds(movePosRotMapping)) {
			Debug.LogWarning("Cannot set target position - Out of range of Arm");
			return;
		}

		animSettingsManager.getCurrentAnimationSetup().Add(movePosRotMapping);
	}

	[Command]
	public void CmdSetLockPosition(PosRotMapping lockTargetPosRot) {
		SetLockPosition(lockTargetPosRot);
	}

	private void SetLockPosition(PosRotMapping lockTargetPosRot) {
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

		int setupCount = animSettingsManager.getCurrentAnimationSetup().Count;
		if (setupCount > 1) {
			animSettingsManager.getCurrentAnimationSetup()[setupCount - 1] = lockTargetPosRot;
		} else {
			animSettingsManager.getCurrentAnimationSetup().Add(lockTargetPosRot);
		}
	}

	public bool isTargetInBounds(PosRotMapping targetPosRotMapping) {
		GameObject tableObject = ObjectManager.Instance.getFirstObjectByName("Table");

		if (tableObject != null) {
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

			if ( CharacterManager.activePatientInstance != null) {
				float armLength = CharacterManager.activePatientInstance.activeArmAnimationController.calculateArmLength() + CharacterManager.activePatientInstance.activeArmAnimationController.getArmRangeSlack();
				float targetDistance = Vector3.Distance(targetPosRotMapping.position, CharacterManager.activePatientInstance.activeArmAnimationController.getArmRangePosition());
				if (targetDistance > armLength) {
					Debug.LogWarning("Arm cannot reach object, too far away: " + targetDistance + "m > " + armLength + "m");
					return false;
				}
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

	private Quaternion ReflectRotation(Quaternion source, Vector3 normal) {
		return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
	}

	// https://gamedevbeginner.com/how-to-spawn-an-object-in-unity-using-instantiate/
	// https://mirror-networking.gitbook.io/docs/guides/gameobjects/spawning-gameobjects
	private void spawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType) {
		for (int i = 0; i < targetPrefabs.Count; i++) {
			if (targetPrefabs[i].name.Equals(_newAnimType.ToString())) {
				float halfHeight = targetPrefabs[i].GetComponent<Renderer>().bounds.extents.y;
				Vector3 rotation = targetPrefabs[i].transform.rotation.eulerAngles;

				// if animation is Key AND patient is left handed, we flip the key
				if (CharacterManager.activePatientInstance != null && _newAnimType == AnimationType.Key && CharacterManager.activePatientInstance.isLeftArmAnimated) {
					rotation = new Vector3(-90f, 0f, -90f);
				}

				GameObject newObject = Instantiate(targetPrefabs[i], spawnArea.transform.position + new Vector3(0, halfHeight, 0), Quaternion.Euler(rotation));
				newObject.gameObject.name = targetPrefabs[i].name;

				NetworkServer.Spawn(newObject);

				SetAnimationStartPosition();
			}

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

					SetLockPosition(new PosRotMapping(newLock1.GetComponent<TargetUtility>().customTargetPos.transform));
				}
			}
		}

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
				GameObject newObject = Instantiate(targetPrefabs[i], spawnArea.transform.position, spawnArea.transform.rotation);
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
	*
	*/

	[Command]
	public void CmdStartAnimationShowcase() {
		RpcStartActualAnimation(true);
	}

	[Command]
	public void CmdStartAnimation() {
		RpcStartActualAnimation(false);
	}

	[Command]
	public void CmdStopAnimation() {
		RpcStopActualAnimation();
	}

	[ClientRpc]
	public void RpcStartActualAnimation(bool isShowcase) {
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

	[Command]
	public void CmdSetArmRestPosition(NetworkIdentity patientIdentity) {
		patientIdentity.gameObject.GetComponent<CharacterManager>().isArmResting = !patientIdentity.gameObject.GetComponent<CharacterManager>().isArmResting;
	}
	

	/*
	*
	* Setting up table
	*
	*/
	
	[Command]
	public void CmdMoveTable(Vector3 offset, NetworkIdentity caller) {
		List<GameObject> tableObjs =  ObjectManager.Instance.getObjectsByName("Table");
		if (tableObjs.Count == 0) {
			return;
		}
		foreach (var item in tableObjs) {
			item.transform.position += offset;
		}

		// we have to complete change object on position, otherwise it won't be synced to clients
		SyncList<PosRotMapping> currentSetup = animSettingsManager.getCurrentAnimationSetup();
		for (int i = 0; i < currentSetup.Count; i++) {
			currentSetup[i] = new PosRotMapping(currentSetup[i].position + offset, currentSetup[i].rotation);
		}
		
		string targetObjectName = animSettingsManager.animType.ToString();

		List<GameObject> targetObjects = ObjectManager.Instance.getObjectsByName(targetObjectName);
		if (targetObjects.Count == 0) {
			return;
		}
		foreach (var item in targetObjects) {
			setItemAuthority(item.GetComponent<NetworkIdentity>(), caller);
		}

		TargetMoveObject(caller.connectionToClient, offset);
	}

	[TargetRpc]
	public void TargetMoveObject(NetworkConnection connection, Vector3 offset) {
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
}
