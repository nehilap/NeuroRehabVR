using System.Collections.Generic;
using Enums;
using Mappings;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

public class NetworkCharacterManager : NetworkBehaviour {

	public static NetworkCharacterManager localNetworkClient;

    [SerializeField]
    private AnimationSettingsManager animSettingsManager;
    
    [SerializeField]
	private List<GameObject> targetPrefabs = new List<GameObject>();

    [SerializeField]
	private GameObject spawnArea;


    void Start() {
		spawnArea = GameObject.Find("SpawnArea");

        try {
			animSettingsManager = GameObject.Find("AnimationSettingsObject").GetComponent<AnimationSettingsManager>();
		}
		catch (System.Exception e) {
			Debug.Log(e);
		}
		
        if (isLocalPlayer) {
			spawnCorrectTargetFakes(AnimationType.Off, animSettingsManager.animType, true);
        }
    }

    public override void OnStartClient () {
		base.OnStartClient();

		if (isLocalPlayer) {
			localNetworkClient = this;
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
        GameObject targetObject = GameObject.Find(animType);
		if (!targetObject) {
			 targetObject = GameObject.Find(animType + "(Clone)");
		}
        if (!targetObject) {
            Debug.LogError("Failed to find object: " + animType);
            return null;
        }
        return new PosRotMapping(targetObject.transform.position, targetObject.transform.rotation.eulerAngles);
    }

	[Command]
	public void CmdAddMovePosition() {
        GameObject originalTargetObject = GameObject.Find(animSettingsManager.animType.ToString());
		if (!originalTargetObject) {
			 originalTargetObject = GameObject.Find(animSettingsManager.animType.ToString() + "(Clone)");
		}

		PosRotMapping _endPositionRotation = new PosRotMapping(originalTargetObject.transform.position, originalTargetObject.transform.rotation.eulerAngles);
		animSettingsManager.getCurrentAnimationSetup().Add(new PosRotMapping(originalTargetObject.transform.position, originalTargetObject.transform.rotation.eulerAngles));
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
		Debug.Log("Spawning object: '"+_newAnimType+"', old object: '"+_oldAnimType+"'");
		if (_newAnimType == _oldAnimType) {
			Debug.Log("Animation types equal, cancelling!");
			return;
		}

		List<GameObject> targetsInScene = FindTargetsOfTypeAll();
		bool foundTarget = false;
		for (int i = 0; i < targetsInScene.Count; i++) {
			if (targetsInScene[i].name.Equals(_newAnimType.ToString())) {
				foundTarget = true;
				break;
			}
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
					GameObject newObject = Instantiate(targetPrefabs[i], spawnArea.transform.position, spawnArea.transform.rotation);
					newObject.gameObject.name = targetPrefabs[i].name;

					NetworkServer.Spawn(newObject);
					
                    CmdSetAnimationStartPosition();
				}
			}
		} else {
			List<GameObject> targetsInScene = FindTargetsOfTypeAll();
			for (int i = 0; i < targetsInScene.Count; i++) {
				if (targetsInScene[i].name.Equals(_newAnimType.ToString())
					|| targetsInScene[i].name.Equals(_newAnimType.ToString() + "(Clone)")) {
					targetsInScene[i].SetActive(true);
				}
			}
		}
		
		GameObject obj = GameObject.Find(_oldAnimType.ToString());
		if (obj) {
			NetworkServer.Destroy(obj);
		}
		obj = GameObject.Find(_oldAnimType.ToString() + "(Clone)");
		if (obj) {
			NetworkServer.Destroy(obj);
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
			List<GameObject> targetsInScene = FindTargetsOfTypeAll();
			for (int i = 0; i < targetsInScene.Count; i++) {
				if (targetsInScene[i].name.Equals(_newAnimType.ToString()) 
					|| targetsInScene[i].name.Equals(_newAnimType.ToString() + "(Clone)") 
					|| targetsInScene[i].name.Equals(_newAnimType.ToString() + "_fake")) {
					targetsInScene[i].SetActive(true);
				}
			}
		}

		// We de-activate both here on client, active state is not synced
		GameObject obj = GameObject.Find(_oldAnimType.ToString() + "_fake");
		if (obj) {
			Destroy(obj);
		}
	}

	public static List<GameObject> FindTargetsOfTypeAll() {
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

	[Command]
	public void CmdSetArmRestPosition() {
		RpcSetArmRestPosition();
	}

	[ClientRpc]
	public void RpcStartActualAnimation(bool isShowcase) {
		if (CharacterManager.activePatient == null) {
			return;
		}
		CharacterManager.activePatient.activeArmAnimationController.startAnimation(isShowcase);
	}

	[ClientRpc]
	public void RpcStopActualAnimation() {
		if (CharacterManager.activePatient == null) {
			return;
		}

		CharacterManager.activePatient.activeArmAnimationController.stopAnimation();
	}
	
	[ClientRpc]
	public void RpcSetArmRestPosition() {
		if (CharacterManager.activePatient == null) {
			return;
		}

		CharacterManager.activePatient.activeArmAnimationController.setArmRestPosition();
	}

	/*
	*
	* Setting up table
	*
	*/
	
	[Command]
	public void CmdMoveTable(Vector3 offset, NetworkIdentity caller) {
		RpcMoveTable(offset);
		
		string targetObjectName = animSettingsManager.animType.ToString();

		GameObject targetObject = GameObject.Find(targetObjectName);
		if (targetObject == null) {
			targetObject = GameObject.Find(targetObjectName + "(Clone)");
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

		GameObject targetObject = GameObject.Find(targetObjectName);
		if (targetObject == null) {
			targetObject = GameObject.Find(targetObjectName + "(Clone)");
		}
		if (targetObject == null) {
			return;
		}

		targetObject.transform.position += offset;
	}

	[ClientRpc]
	public void RpcMoveTable(Vector3 offset) {
		GameObject table = GameObject.Find("Table");
		if (table == null) {
			return;
		}

		table.transform.position += offset;

		if (CharacterManager.activePatient == null) {
			return;
		}
		CharacterManager.activePatient.activeArmAnimationController.alignArmRestTargetWithTable();
	}
}
