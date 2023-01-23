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
	* COMMANDS FOR SERVER
	* 
	*/

	[Command]
	public void CMDUpdateArmDuration(float value) {
		if (animSettingsManager.armMoveDuration == value) return;

		animSettingsManager.armMoveDuration = value;
	}

	[Command]
	public void CMDUpdateHandDuration(float value) {
		if (animSettingsManager.handMoveDuration == value) return;

		animSettingsManager.handMoveDuration = value;
	}

	[Command]
	public void CMDUpdateWaitDuration(float value) {
		if (animSettingsManager.waitDuration == value) return;
		
		animSettingsManager.waitDuration = value;
	}
	
	[Command]
	public void CMDUpdateMoveDuration(float value) {
		if (animSettingsManager.moveDuration == value) return;
		
		animSettingsManager.moveDuration = value;
	}
	
	[Command]
	public void CMDUpdateRepetitions(int value) {
		if (animSettingsManager.repetitions == value) return;
		
		animSettingsManager.repetitions = value;
	}

	[Command]
	public void CMDUpdateAnimType(AnimationType _animType) {
		if (animSettingsManager.animType == _animType) return;
		
		animSettingsManager.animType = _animType;
	}

	/*
	*
	* SYNCING START / END POSITIONS
	*
	*/

    [Command]
	public void CMDSetAnimationStartPosition(PosRotMapping newPosRotMapping) {
        if (newPosRotMapping == null) {
            newPosRotMapping = getAnimationStartPositionFromObject();
            if (newPosRotMapping == null) {
                return;
            }
        }

        if (animSettingsManager.getCurrentAnimationSetup().Count >= 1) {
            animSettingsManager.getCurrentAnimationSetup()[0] = newPosRotMapping;
        } else {
            animSettingsManager.getCurrentAnimationSetup().Add(newPosRotMapping);
        }
	}

    public PosRotMapping getAnimationStartPositionFromObject() {
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
	public void CMDAddMovePosition() {
        GameObject originalTargetObject = GameObject.Find(animSettingsManager.animType.ToString());
		if (!originalTargetObject) {
			 originalTargetObject = GameObject.Find(animSettingsManager.animType.ToString() + "(Clone)");
		}

		PosRotMapping _endPositionRotation = new PosRotMapping(originalTargetObject.transform.position, originalTargetObject.transform.rotation.eulerAngles);
		animSettingsManager.getCurrentAnimationSetup().Add(new PosRotMapping(originalTargetObject.transform.position, originalTargetObject.transform.rotation.eulerAngles));
	}

	[Command]
	public void CMDClearAnimationMovePositions() {
		animSettingsManager.getCurrentAnimationSetup().Clear();
	}

    /**
	*
	* TARGET OBJECT SPAWNING
	*
	*/

	[Command]
	public void CMDSpawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType) {
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

		RPCSpawnCorrectTarget(_oldAnimType, _newAnimType, !foundTarget);

		spawnCorrectTarget(_oldAnimType, _newAnimType, !foundTarget);
	}

	[ClientRpc]
	public void RPCSpawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType, bool spawnNew) {
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
					
                    CMDSetAnimationStartPosition(new PosRotMapping(newObject.transform.position, newObject.transform.rotation.eulerAngles));
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
}
