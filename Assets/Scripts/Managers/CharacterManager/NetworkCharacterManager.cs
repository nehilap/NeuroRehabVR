using System.Collections.Generic;
using Enums;
using Mappings;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

public class NetworkCharacterManager : NetworkBehaviour {

	public static NetworkCharacterManager localNetworkClientInstance { get; private set; }

	[SerializeField] private AnimationSettingsManager animSettingsManager;
	
	[SerializeField] private List<GameObject> targetPrefabs = new List<GameObject>();

	[SerializeField] private GameObject spawnArea;

	void Start() {
		spawnArea = GameObject.Find("SpawnArea");

		try {
			animSettingsManager = GameObject.Find("AnimationSettingsObject").GetComponent<AnimationSettingsManager>();
		} catch (System.Exception e) {
			Debug.Log(e);
		}
		
		if (isLocalPlayer) {
			spawnCorrectTargetFakes(AnimationType.Off, animSettingsManager.animType, true);
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

		PosRotMapping newPosRotMapping = getPosRotFromObject();
		if (newPosRotMapping == null) {
			return;
		}

		if (animSettingsManager.getCurrentAnimationSetup().Count >= 1) {
			animSettingsManager.getCurrentAnimationSetup()[0] = newPosRotMapping;
		} else {
			animSettingsManager.getCurrentAnimationSetup().Add(newPosRotMapping);
		}
	}

	private PosRotMapping getPosRotFromObject() {
		string animType = animSettingsManager.animType.ToString();
		GameObject targetObject = ObjectManager.Instance.getTargetByName(animType);
		if (!targetObject) {
			 targetObject = ObjectManager.Instance.getTargetByName(animType + "(Clone)");
		}
		if (!targetObject) {
			Debug.LogError("Failed to find object: " + animType);
			return null;
		}
		return new PosRotMapping(targetObject.transform.position, targetObject.transform.rotation.eulerAngles);
	}

	[Command]
	public void CmdAddMovePosition() {
		GameObject originalTargetObject = ObjectManager.Instance.getTargetByName(animSettingsManager.animType.ToString());
		if (!originalTargetObject) {
			originalTargetObject = ObjectManager.Instance.getTargetByName(animSettingsManager.animType.ToString() + "(Clone)");
		}

		PosRotMapping _endPositionRotation = new PosRotMapping(originalTargetObject.transform.position, originalTargetObject.transform.rotation.eulerAngles);
		animSettingsManager.getCurrentAnimationSetup().Add(_endPositionRotation);
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

		int setupCount = animSettingsManager.getCurrentAnimationSetup().Count;
		if (setupCount > 1) {
			animSettingsManager.getCurrentAnimationSetup()[setupCount - 1] = lockTargetPosRot;
		} else {
			animSettingsManager.getCurrentAnimationSetup().Add(lockTargetPosRot);
		}
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

		bool foundTarget = false;
		if (ObjectManager.Instance.getTargetByName(_newAnimType.ToString()) != null || ObjectManager.Instance.getTargetByName(_newAnimType.ToString() + "(Clone)") != null) {
			foundTarget = true;
		}

		RpcSpawnCorrectTarget(_oldAnimType, _newAnimType, !foundTarget);

		spawnCorrectTarget(_oldAnimType, _newAnimType, !foundTarget);
	}

	[ClientRpc]
	public void RpcSpawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType, bool spawnNew) {
		spawnCorrectTargetFakes(_oldAnimType, _newAnimType, spawnNew);
	}

	// https://gamedevbeginner.com/how-to-spawn-an-object-in-unity-using-instantiate/
	// https://mirror-networking.gitbook.io/docs/guides/gameobjects/spawning-gameobjects
	private void spawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType, bool spawnNew) {
		if (spawnNew) {
			for (int i = 0; i < targetPrefabs.Count; i++) {
				if (targetPrefabs[i].name.Equals(_newAnimType.ToString())) {
					float halfHeight = targetPrefabs[i].transform.lossyScale.y * targetPrefabs[i].GetComponent<MeshFilter>().sharedMesh.bounds.extents.y;
					GameObject newObject = Instantiate(targetPrefabs[i], spawnArea.transform.position + new Vector3(0, halfHeight, 0), targetPrefabs[i].transform.rotation);
					newObject.gameObject.name = targetPrefabs[i].name;

					NetworkServer.Spawn(newObject);

					SetAnimationStartPosition();
				}

				if (_newAnimType == AnimationType.Key) {
					if (targetPrefabs[i].name.Equals("Lock")) {
						float halfHeight = targetPrefabs[i].transform.lossyScale.y * targetPrefabs[i].GetComponent<MeshFilter>().sharedMesh.bounds.extents.y;

						// Center
						GameObject newLock1 = Instantiate(targetPrefabs[i], spawnArea.transform.position + new Vector3(0, halfHeight, 0.22f), targetPrefabs[i].transform.rotation);
						NetworkServer.Spawn(newLock1);

						// Left
						GameObject newLock2 = Instantiate(targetPrefabs[i], spawnArea.transform.position + new Vector3(-0.3f, halfHeight, 0.15f), targetPrefabs[i].transform.rotation);
						newLock2.transform.LookAt(spawnArea.transform.position + new Vector3(0, halfHeight, 0));
						NetworkServer.Spawn(newLock2);

						// Right
						GameObject newLock3 = Instantiate(targetPrefabs[i], spawnArea.transform.position + new Vector3(+0.3f, halfHeight, 0.15f), targetPrefabs[i].transform.rotation);
						newLock3.transform.LookAt(spawnArea.transform.position + new Vector3(0, halfHeight, 0));
						NetworkServer.Spawn(newLock3);

						SetLockPosition(new PosRotMapping(newLock1.GetComponent<TargetUtility>().customTargetPos.transform));
					}
				}
			}
		} else {
			Dictionary<string, GameObject> targetsInScene = ObjectManager.Instance.getTargetList();
			foreach (var item in targetsInScene) {
				if (item.Key.Contains(_newAnimType.ToString())) {
					item.Value.SetActive(true);
				}
				if (_newAnimType == AnimationType.Key) {
					if (item.Key.Contains("Lock")) {
						item.Value.SetActive(true);
					}
				}
			}
		}
		
		GameObject obj = ObjectManager.Instance.getTargetByName(_oldAnimType.ToString());
		if (obj) {
			NetworkServer.Destroy(obj);
		}
		obj = ObjectManager.Instance.getTargetByName(_oldAnimType.ToString() + "(Clone)");
		if (obj) {
			NetworkServer.Destroy(obj);
		}

		if (_oldAnimType == AnimationType.Key) {
			Dictionary<string, GameObject> targetsInScene = ObjectManager.Instance.getTargetList();
			foreach (var item in targetsInScene) {
				if (item.Key.Contains("Lock")) {
					NetworkServer.Destroy(item.Value);
				}
			}
		}
	}

	private void spawnCorrectTargetFakes(AnimationType _oldAnimType, AnimationType _newAnimType, bool spawnNew) {
		if (spawnNew) {
			for (int i = 0; i < targetPrefabs.Count; i++) {
				if (targetPrefabs[i].name.Equals(_newAnimType.ToString() + "_fake")) {
					GameObject newObject = Instantiate(targetPrefabs[i], spawnArea.transform.position, spawnArea.transform.rotation);
					newObject.gameObject.name = targetPrefabs[i].name;
				}
			}
		} else {
			// We activate both here on client, active state is not synced
			// we check both names for objects, because server spawned objects have (Clone) in the name
			Dictionary<string, GameObject> targetsInScene = ObjectManager.Instance.getTargetList();
			foreach (var item in targetsInScene) {
				if (item.Key.Contains(_newAnimType.ToString())) {
					item.Value.SetActive(true);
				}
			}
		}

		// We de-activate fake here on client
		GameObject obj = ObjectManager.Instance.getTargetByName(_oldAnimType.ToString() + "_fake");
		if (obj) {
			Destroy(obj);
		}

/* 		if (_oldAnimType == AnimationType.Key) {
			Dictionary<string, GameObject> targetsInScene = ObjectManager.Instance.getTargetList();
			foreach (var item in targetsInScene) {
				if (item.Key.Contains("Lock") && item.Key.Contains("_fake")) {
					Destroy(item.Value);
				}
			}
		} */
	}

	/*public static List<GameObject> FindTargetsOfTypeAll() {
		List<GameObject> results = new List<GameObject>();
		for(int i = 0; i< SceneManager.sceneCount; i++) {
			var s = SceneManager.GetSceneAt(i);
			if (s.isLoaded) {
				var allGameObjects = s.GetRootGameObjects();
				for (int j = 0; j < allGameObjects.Length; j++) {
					var go = allGameObjects[j];
					TargetUtility[] items = go.GetComponentsInChildren<TargetUtility>(true);
					foreach (TargetUtility item in items) {
						results.Add(item.gameObject);
					}
				}
			}
		}
		return results;
	}*/

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
		GameObject table = GameObject.Find("Table");
		if (table == null) {
			return;
		}

		table.transform.position += offset;

		// we have to complete change object on position, otherwise it won't be synced to clients
		SyncList<PosRotMapping> currentSetup = animSettingsManager.getCurrentAnimationSetup();
		for (int i = 0; i < currentSetup.Count; i++) {
			currentSetup[i] = new PosRotMapping(currentSetup[i].position + offset, currentSetup[i].rotation);
		}
		
		string targetObjectName = animSettingsManager.animType.ToString();

		GameObject targetObject = ObjectManager.Instance.getTargetByName(targetObjectName);
		if (targetObject == null) {
			targetObject = ObjectManager.Instance.getTargetByName(targetObjectName + "(Clone)");
		}
		if (targetObject == null) {
			return;
		}
		setItemAuthority(targetObject.GetComponent<NetworkIdentity>(), caller);

		TargetMoveObject(caller.connectionToClient, offset);
	}

	[TargetRpc]
	public void TargetMoveObject(NetworkConnection connection, Vector3 offset) {
		string targetObjectName = animSettingsManager.animType.ToString();

		GameObject targetObject = ObjectManager.Instance.getTargetByName(targetObjectName);
		if (targetObject == null) {
			targetObject = ObjectManager.Instance.getTargetByName(targetObjectName + "(Clone)");
		}
		if (targetObject == null) {
			return;
		}

		targetObject.transform.position += offset;
	}
}
