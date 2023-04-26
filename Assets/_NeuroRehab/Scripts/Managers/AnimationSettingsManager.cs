using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Enums;
using System.Collections.Generic;
using NeuroRehab.Mappings;
using NeuroRehab.Utility;
using System;
using System.Linq;

[System.Serializable]
public class AnimationSettingsManager : NetworkBehaviour {
	[SyncVar(hook = nameof(changeArmMoveDurationElements))] [Range(0.5f, 5f)]
	public float armMoveDuration = 1.5f;

	[SyncVar(hook = nameof(changeHandMoveDurationElements))] [Range(0.5f, 5f)]
	public float handMoveDuration = 1.5f;

	[SyncVar(hook = nameof(changeWaitDurationElements))] [Range(0.5f, 20f)]
	public float waitDuration = 10f;

	[SyncVar(hook = nameof(changeMoveDurationElements))] [Range(0.5f, 10f)]
	public float moveDuration = 4f;

	[SyncVar(hook = nameof(changeRepetitionsElements))] [Range(1, 20)]
	public int repetitions = 5;

	[SyncVar(hook = nameof(changeAnimTypeValueHook))]
	[SerializeField] private AnimationType _animType = AnimationType.Block;
	public AnimationType animType {
			get { return _animType; }
			private set {
				_animType = value;
			}
		}

	public AnimationType prevAnimType;

	public TMP_Text armMoveTextValue;
	public Slider armMoveSlider;

	public TMP_Text handMoveTextValue;
	public Slider handMoveSlider;

	public TMP_Text waitDurTextValue;
	public Slider waitDurSlider;

	public TMP_Text moveDurTextValue;
	public Slider moveDurSlider;

	public TMP_Text repetitionsTextValue;
	public Slider repetitionsSlider;

	[SerializeField] private Button registerMovePositionButton;

	public TMP_Dropdown animTypeDropdown;

	// https://mirror-networking.gitbook.io/docs/guides/synchronization/synclists
	public readonly SyncList<PosRotMapping> blockSetup = new SyncList<PosRotMapping>();
	public readonly SyncList<PosRotMapping> cubeSetup = new SyncList<PosRotMapping>();
	public readonly SyncList<PosRotMapping> cupSetup = new SyncList<PosRotMapping>();
	public readonly SyncList<PosRotMapping> keySetup = new SyncList<PosRotMapping>();

	[SerializeField] private List<GameObject> markerPrefabs = new List<GameObject>();
	[SerializeField] private Transform markerParent;
	[SerializeField] private List<GameObject> spawnedMarkers = new List<GameObject>();

	[SerializeField] private GameObject spawnArea;

	[SerializeField] public List<GameObject> targetPrefabs = new List<GameObject>();

	private bool dropdownValuesInitialized = false;

	public void Start() {
		spawnArea = ObjectManager.Instance.getFirstObjectByName("SpawnArea");

		if (spawnArea == null) {
			Debug.LogError("'SpawnArea' not found");
			return;
		}

		initDropdown();
		animType = AnimationType.Block;
		prevAnimType = AnimationType.Block;

		if (isClient) {
			setAllElements();

			if (SettingsManager.Instance.roleSettings.characterRole == UserRole.Therapist) {
				blockSetup.Callback += onAnimationSetupUpdated;
				cubeSetup.Callback += onAnimationSetupUpdated;
				cupSetup.Callback += onAnimationSetupUpdated;
				keySetup.Callback += onAnimationSetupUpdated;

				setupMarkers();
			}

			animTypeDropdown.onValueChanged.AddListener(delegate {
				animationTypeDropdownHandler(animTypeDropdown);
			});

			if (SettingsManager.Instance.roleSettings.characterRole == UserRole.Patient) {
				animTypeDropdown.GetComponent<ButtonClickAudio>().enabled = false;
				armMoveSlider.GetComponent<ButtonClickAudio>().enabled = false;
				moveDurSlider.GetComponent<ButtonClickAudio>().enabled = false;
				waitDurSlider.GetComponent<ButtonClickAudio>().enabled = false;
				handMoveSlider.GetComponent<ButtonClickAudio>().enabled = false;
				repetitionsSlider.GetComponent<ButtonClickAudio>().enabled = false;
			}
		}
	}

	public override void OnStartServer() {
		string animTypeString = animType.ToString();
		GameObject targetObject = GameObject.Find(animTypeString);
		if (!targetObject) {
			targetObject = GameObject.Find(animTypeString + "(Clone)");
		}
		if (!targetObject) {
			Debug.LogError("Failed to find object: " + animTypeString);
		} else {
			getCurrentAnimationSetup().Add(new PosRotMapping(targetObject.transform.position, targetObject.transform.rotation.eulerAngles));
		}
	}

	private void initDropdown() {
		if (dropdownValuesInitialized) {
			return;
		}

		animTypeDropdown.ClearOptions();
		animTypeDropdown.AddOptions(Enum.GetNames(typeof(AnimationType)).ToList());
		dropdownValuesInitialized = true;
	}

	public SyncList<PosRotMapping> getCurrentAnimationSetup() {
		switch (animType) {
			case AnimationType.Block:
				return blockSetup;
			case AnimationType.Cube:
				return cubeSetup;
			case AnimationType.Cup:
				return cupSetup;
			case AnimationType.Key:
				return keySetup;
			case AnimationType.Off:
				return new SyncList<PosRotMapping>();
			default: return null; // AnimType.Off
		}
	}

	public SyncList<PosRotMapping> getAnimationSetupByAnimType(AnimationType _animType) {
		switch (_animType) {
			case AnimationType.Block:
				return blockSetup;
			case AnimationType.Cube:
				return cubeSetup;
			case AnimationType.Cup:
				return cupSetup;
			case AnimationType.Key:
				return keySetup;
			case AnimationType.Off:
				return new SyncList<PosRotMapping>();
			default: return null; // AnimType.Off
		}
	}

	public List<SyncList<PosRotMapping>> getAllAnimationSetups() {
		List<SyncList<PosRotMapping>> setups = new List<SyncList<PosRotMapping>>();
		setups.Add(blockSetup);
		setups.Add(cubeSetup);
		setups.Add(cupSetup);
		setups.Add(keySetup);

		return setups;
	}

	/*
	* FUNCTIONS FOR SETTING ELEMENTS VALUES
	*/

	private void changeArmMoveDurationElements(float _old, float _new) {
		armMoveTextValue.text = (Mathf.Round(armMoveDuration * 10) / 10).ToString("F1") + " s";

		armMoveSlider.value = (int) (armMoveDuration * 2);
	}

	private void changeHandMoveDurationElements(float _old, float _new) {
		handMoveTextValue.text = (Mathf.Round(handMoveDuration * 10) / 10).ToString("F1") + " s";

		handMoveSlider.value = (int) (handMoveDuration * 2);
	}

	private void changeWaitDurationElements(float _old, float _new) {
		waitDurTextValue.text = (Mathf.Round(waitDuration * 10) / 10).ToString("F1") + " s";

		waitDurSlider.value = (int) (waitDuration * 2);
	}
	private void changeMoveDurationElements(float _old, float _new) {
		moveDurTextValue.text = (Mathf.Round(moveDuration * 10) / 10).ToString("F1") + " s";

		moveDurSlider.value = (int) (moveDuration * 2);
	}

	private void changeRepetitionsElements(int _old, int _new) {
		repetitionsTextValue.text = repetitions + " x";

		repetitionsSlider.value = repetitions;
	}

	private void changeAnimTypeValueHook(AnimationType _old, AnimationType _new) {
		if (!dropdownValuesInitialized) {
			initDropdown();
		}

		prevAnimType = _old;
		disableObjects(_old);

		animTypeDropdown.value = animTypeDropdown.options.FindIndex(option => option.text == animType.ToString());

		if (animType == AnimationType.Key) {
			registerMovePositionButton.interactable = false;
		} else {
			registerMovePositionButton.interactable = true;
		}

		setupMarkers();
	}

	/// <summary>
	/// Helper method used for disabling colliders on old object to prevent new spawned to "bump" into them and be offset. This happens due to lag when information is delivered from server.
	/// </summary>
	/// <param name="_oldAnimType"></param>
	private void disableObjects(AnimationType _oldAnimType) {
		List<GameObject> oldTargetsInScene = ObjectManager.Instance.getObjectsByName(_oldAnimType.ToString());
		foreach (var item in oldTargetsInScene) {
			item.GetComponent<Renderer>().enabled = false;
			item.GetComponent<Collider>().enabled = false;
		}

		if (_oldAnimType == AnimationType.Key) {
			oldTargetsInScene = ObjectManager.Instance.getObjectsByName("Lock");
			foreach (var item in oldTargetsInScene) {
				item.GetComponent<Renderer>().enabled = false;
				item.GetComponent<Collider>().enabled = false;
			}
		}
	}

	/// <summary>
	/// Changes animation type only if possible (activePatient && animation not playing)
	/// </summary>
	/// <param name="_old">Previous (current) animation type</param>
	/// <param name="_new">New animation type</param>
	/// <returns>Bool value depending on whether animation type was changed or not</returns>
	[Server]
	public bool setAnimType(AnimationType _old, AnimationType _new) {
		if (AnimationServerManager.Instance.isTrainingRunning || (CharacterManager.activePatientInstance != null && CharacterManager.activePatientInstance.activeArmAnimationController.animState == Enums.AnimationState.Playing)) {
			return false;
		}
		if (_old != AnimationType.Off) {
			prevAnimType = _old;
		}
		animType = _new;
		return true;
	}

	/// <summary>
	/// Method used to initialize UI elements: sliders, text and dropdown. Needed because when user connects, we have to set synced values to elements.
	/// </summary>
	private void setAllElements() {
		armMoveTextValue.text = (Mathf.Round(armMoveDuration * 10) / 10).ToString("F1") + " s";
		armMoveSlider.value = (int) (armMoveDuration * 2);

		handMoveTextValue.text = (Mathf.Round(handMoveDuration * 10) / 10).ToString("F1") + " s";
		handMoveSlider.value = (int) (handMoveDuration * 2);

		waitDurTextValue.text = (Mathf.Round(waitDuration * 10) / 10).ToString("F1") + " s";
		waitDurSlider.value = (int) (waitDuration * 2);

		moveDurTextValue.text = (Mathf.Round(moveDuration * 10) / 10).ToString("F1") + " s";
		moveDurSlider.value = (int) (moveDuration * 2);

		repetitionsTextValue.text = repetitions + " x";
		repetitionsSlider.value = repetitions;

		animTypeDropdown.value = animTypeDropdown.options.FindIndex(option => option.text == animType.ToString());
	}

	/// <summary>
	/// Method used for handling changes made to SyncList. Refer to https://mirror-networking.gitbook.io/docs/manual/guides/synchronization/synclists for more information.
	/// </summary>
	/// <param name="op"></param>
	/// <param name="index"></param>
	/// <param name="oldItem"></param>
	/// <param name="newItem"></param>
	void onAnimationSetupUpdated(SyncList<PosRotMapping>.Operation op, int index, PosRotMapping oldItem, PosRotMapping newItem) {
		if (SettingsManager.Instance.roleSettings.characterRole == UserRole.Patient) {
			return;
		}

		int prefabIndex;
		// Debug.Log(newItem);
		switch (op) {
			case SyncList<PosRotMapping>.Operation.OP_ADD:
				// index is where it was added into the list
				// newItem is the new item
				// Debug.Log("Item added");
				prefabIndex = index == 0 ? 0 : 1;

				GameObject marker = GameObject.Instantiate(markerPrefabs[prefabIndex], newItem.position, Quaternion.Euler(newItem.rotation), markerParent) as GameObject;
				if (marker.TryGetComponent<MarkerNumber>(out MarkerNumber markerNumber)) {
					markerNumber.orderString = $"{index + 1}";
				}

				spawnedMarkers.Add(marker);
				break;
			case SyncList<PosRotMapping>.Operation.OP_INSERT:
				// index is where it was inserted into the list
				// newItem is the new item
				break;
			case SyncList<PosRotMapping>.Operation.OP_REMOVEAT:
				// index is where it was removed from the list
				// oldItem is the item that was removed
				if (spawnedMarkers.Count > index) {
					GameObject.Destroy(spawnedMarkers[index]);
					spawnedMarkers.RemoveAt(index);
				}
				break;
			case SyncList<PosRotMapping>.Operation.OP_SET:
				// index is of the item that was changed
				// oldItem is the previous value for the item at the index
				// newItem is the new value for the item at the index
				// Debug.Log("Item changed");
				if (spawnedMarkers.Count > index) {
					GameObject.Destroy(spawnedMarkers[index]);
				}
				prefabIndex = index == 0 ? 0 : 1;

				GameObject newMarker = GameObject.Instantiate(markerPrefabs[prefabIndex], newItem.position, Quaternion.Euler(newItem.rotation), markerParent) as GameObject;
				if (newMarker.TryGetComponent<MarkerNumber>(out MarkerNumber newMarkerNumber)) {
					newMarkerNumber.orderString = $"{index + 1}";
				}

				if (spawnedMarkers.Count > index) {
					spawnedMarkers[index] = newMarker;
				} else {
					spawnedMarkers.Add(newMarker);
				}
				break;
			case SyncList<PosRotMapping>.Operation.OP_CLEAR:
				// list got cleared
				// Debug.Log("List cleared");
				foreach (GameObject item in spawnedMarkers) {
					GameObject.Destroy(item);
				}

				spawnedMarkers.Clear();
				break;
		}
	}

	/// <summary>
	/// Method that re-creates all markers locally only
	/// </summary>
	private void setupMarkers() {
		if (SettingsManager.Instance.roleSettings.characterRole == UserRole.Patient) {
			return;
		}

		foreach (GameObject item in spawnedMarkers) {
			GameObject.Destroy(item);
		}
		spawnedMarkers.Clear();

		if (animType == AnimationType.Off) {
			return;
		}

		SyncList<PosRotMapping> currentMapping = getCurrentAnimationSetup();

		if (currentMapping.Count > 0) {
			GameObject marker = GameObject.Instantiate(markerPrefabs[0], currentMapping[0].position, Quaternion.Euler(currentMapping[0].rotation), markerParent) as GameObject;
			if (marker.TryGetComponent<MarkerNumber>(out MarkerNumber markerNumber)) {
				markerNumber.orderString = $"{1}";
			}
			spawnedMarkers.Add(marker);

			for (int i = 1; i < currentMapping.Count; i++) {
				marker = GameObject.Instantiate(markerPrefabs[1], currentMapping[i].position, Quaternion.Euler(currentMapping[i].rotation), markerParent) as GameObject;
				if (marker.TryGetComponent<MarkerNumber>(out MarkerNumber newMarkerNumber)) {
					newMarkerNumber.orderString = $"{i + 1}";
				}
				spawnedMarkers.Add(marker);
			}
		}
	}

	/*
	* HANDLERS for sliders, need to be assigned in editor
	* We have to take in float values, because that's default type that slider works with
	*/

	public void armDurationSliderHandler(float value) {
		if (isServer) {
			return;
		}

		armMoveDuration = value / 2f;

		changeArmMoveDurationElements(value, value);
		NetworkCharacterManager.localNetworkClientInstance.CmdUpdateArmDuration(armMoveDuration);
	}

	public void handDurationSliderHandler(float value) {
		if (isServer) {
			return;
		}

		handMoveDuration = value / 2f;

		changeHandMoveDurationElements(value, value);
		NetworkCharacterManager.localNetworkClientInstance.CmdUpdateHandDuration(handMoveDuration);
	}

	public void waitDurationSliderHandler(float value) {
		if (isServer) {
			return;
		}

		waitDuration = value / 2f;

		changeWaitDurationElements(value, value);
		NetworkCharacterManager.localNetworkClientInstance.CmdUpdateWaitDuration(waitDuration);
	}

	public void moveDurationSliderHandler(float value) {
		if (isServer) {
			return;
		}

		moveDuration = value / 2f;

		changeMoveDurationElements(value, value);
		NetworkCharacterManager.localNetworkClientInstance.CmdUpdateMoveDuration(moveDuration);
	}

	public void repetitionsSliderHandler(float value) {
		if (isServer) {
			return;
		}

		repetitions = (int) value;

		changeRepetitionsElements((int) value, (int) value);
		NetworkCharacterManager.localNetworkClientInstance.CmdUpdateRepetitions(repetitions);
	}

	public void animationTypeDropdownHandler(TMP_Dropdown dropdown) {
		if (isServer) {
			return;
		}

		AnimationType tmpAnimType = AnimationType.Off;
		prevAnimType = animType;

		switch (dropdown.options[dropdown.value].text) {
			case "Off": tmpAnimType = AnimationType.Off; break;
			case "Cube": tmpAnimType = AnimationType.Cube; break;
			case "Cup": tmpAnimType = AnimationType.Cup; break;
			case "Key": tmpAnimType = AnimationType.Key; break;
			case "Block": tmpAnimType = AnimationType.Block; break;
			default: return;
		}

		if (NetworkCharacterManager.localNetworkClientInstance != null && prevAnimType != tmpAnimType) {
			NetworkCharacterManager.localNetworkClientInstance.CmdUpdateAnimType(prevAnimType, tmpAnimType);
			// NetworkCharacterManager.localNetworkClientInstance.CmdSpawnCorrectTarget(prevAnimType, tmpAnimType);
		}
	}

	[ClientRpc]
	public void RpcSpawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType) {
		spawnCorrectTargetFakes(_oldAnimType, _newAnimType);
	}

	/// <summary>
	/// Method called on server, old objects are then destroyed and new spawned on all clients. Refer to https://mirror-networking.gitbook.io/docs/guides/gameobjects/spawning-gameobjects for more details.
	/// </summary>
	/// <param name="_oldAnimType"></param>
	/// <param name="_newAnimType"></param>
	[Server]
	public void spawnCorrectTarget(AnimationType _oldAnimType, AnimationType _newAnimType) {
		Debug.Log("Spawning object: '" + _newAnimType + "', old object: '" + _oldAnimType + "'");

		// destroy old objects
		List<GameObject> oldTargetsInScene = ObjectManager.Instance.getObjectsByName(_oldAnimType.ToString());
		foreach (var item in oldTargetsInScene) {
			item.GetComponent<Collider>().enabled = false;
			NetworkServer.Destroy(item);
		}
		if (_oldAnimType == AnimationType.Key) {
			oldTargetsInScene = ObjectManager.Instance.getObjectsByName("Lock");
			foreach (var item in oldTargetsInScene) {
				NetworkServer.Destroy(item);
			}
		}

		if (_newAnimType == AnimationType.Off) {
			return;
		}

		GameObject targetObject = targetPrefabs.Find((x) => x.name.Equals(_newAnimType.ToString()));
		if (targetObject == null) {
			return;
		}

		float halfHeight = targetObject.GetComponent<Renderer>().bounds.extents.y;
		Vector3 rotation = targetObject.transform.rotation.eulerAngles;

		// if animation is Key AND patient is left handed, we flip the key
		if (CharacterManager.activePatientInstance != null && _newAnimType == AnimationType.Key && CharacterManager.activePatientInstance.isLeftArmAnimated) {
			rotation = new Vector3(-90f, 0f, -90f); // did not find effective algorithm to mirror the key, so it is what it is
		}
		GameObject newObject = Instantiate(targetObject, spawnArea.transform.position + new Vector3(0, halfHeight, 0), Quaternion.Euler(rotation));
		newObject.gameObject.name = targetObject.name;

		NetworkServer.Spawn(newObject);
		setAnimationStartPosition(false);

		if (_newAnimType != AnimationType.Key) {
			return;
		}

		GameObject lockObject = targetPrefabs.Find((x) => x.name.Equals("Lock"));
		if (lockObject == null) {
			return;
		}
		// we spawn three locks in this case
		halfHeight = lockObject.transform.lossyScale.y * lockObject.GetComponent<MeshFilter>().sharedMesh.bounds.extents.y;

		// Center
		GameObject newLock1 = Instantiate(lockObject, spawnArea.transform.position + new Vector3(0, halfHeight, 0.25f), lockObject.transform.rotation);
		NetworkServer.Spawn(newLock1);
		// Left
		GameObject newLock2 = Instantiate(lockObject, spawnArea.transform.position + new Vector3(-0.3f, halfHeight, 0.18f), lockObject.transform.rotation);
		newLock2.transform.LookAt(spawnArea.transform.position + new Vector3(0, halfHeight, 0));
		NetworkServer.Spawn(newLock2);
		// Right
		GameObject newLock3 = Instantiate(lockObject, spawnArea.transform.position + new Vector3(+0.3f, halfHeight, 0.18f), lockObject.transform.rotation);
		newLock3.transform.LookAt(spawnArea.transform.position + new Vector3(0, halfHeight, 0));
		NetworkServer.Spawn(newLock3);

		setLockPosition(new PosRotMapping(newLock1.GetComponent<TargetUtility>().customTargetPos.transform));
	}

	/// <summary>
	/// Function used to destroy old objects and spawn new. Used only on client.
	/// </summary>
	/// <param name="_oldAnimType"></param>
	/// <param name="_newAnimType"></param>
	[Client]
	public void spawnCorrectTargetFakes(AnimationType _oldAnimType, AnimationType _newAnimType) {
		// We destroy fake objects here on client
		List<GameObject> oldTargetsInScene = ObjectManager.Instance.getObjectsByName(_oldAnimType.ToString() + "_fake");
		foreach (var item in oldTargetsInScene) {
			Destroy(item);
		}

		// We destroy normal objact on client, in case they did not get destroyed by server
		oldTargetsInScene = ObjectManager.Instance.getObjectsByName(_oldAnimType.ToString());
		foreach (var item in oldTargetsInScene) {
			if (!item.activeSelf) {
				Destroy(item);
			}
		}

		if (_newAnimType == AnimationType.Off) {
			return;
		}

		GameObject targetObject = targetPrefabs.Find((x) => x.name.Equals(_newAnimType.ToString() + "_fake"));
		if (targetObject == null) {
			return;
		}

		Vector3 rotation = targetObject.transform.rotation.eulerAngles;

		// if animation is Key AND patient is left handed, we flip the key
		if (CharacterManager.activePatientInstance != null && _newAnimType == AnimationType.Key && CharacterManager.activePatientInstance.isLeftArmAnimated) {
			rotation = new Vector3(-90f, 0f, -90f); // did not find effective algorithm to mirror the key, so it is what it is
		}
		GameObject newObject = Instantiate(targetObject, spawnArea.transform.position, Quaternion.Euler(rotation));
		newObject.gameObject.name = targetObject.name;
	}

	/// <summary>
	/// Function used for changing or adding start position for current chosen setup. Uses current position and rotation of target object
	/// </summary>
	/// <param name="forceNew">Used to determine whether we want to force rewrite first position no matter what</param>
	[Server]
	public void setAnimationStartPosition(bool forceNew) {
		if (!isServer) {
			return;
		}
		if (!forceNew && getCurrentAnimationSetup().Count >= 1) {
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
		if (getCurrentAnimationSetup().Count >= 1) {
			getCurrentAnimationSetup()[0] = targetPosRotMapping;
		} else {
			getCurrentAnimationSetup().Add(targetPosRotMapping);
		}
	}

	/// <summary>
	/// Function used for changing or adding start position for current chosen setup. Uses current custom position and rotation
	/// </summary>
	/// <param name="forceNew">Used to determine whether we want to force rewrite first position no matter what</param>
	/// <param name="targetPosRotMapping">Custom position and rotation</param>
	[Server]
	public void setAnimationStartPosition(bool forceNew, PosRotMapping targetPosRotMapping) {
		if (!isServer) {
			return;
		}
		if (!forceNew && getCurrentAnimationSetup().Count >= 1) {
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
		if (getCurrentAnimationSetup().Count >= 1) {
			getCurrentAnimationSetup()[0] = targetPosRotMapping;
		} else {
			getCurrentAnimationSetup().Add(targetPosRotMapping);
		}
	}


	/// <summary>
	/// Sets the lock position at the end of synclist
	/// </summary>
	/// <param name="lockTargetPosRot"></param>
	[Server]
	public void setLockPosition(PosRotMapping lockTargetPosRot) {
		if (!isServer) {
			return;
		}

		if (animType != AnimationType.Key) {
			return;
		}

		if (!isTargetInBounds(lockTargetPosRot)) {
			Debug.LogWarning("Cannot set Lock position - Out of range of Arm");
			return;
		}

		// we only allow 2 positions in case of Key animation
		int setupCount = getCurrentAnimationSetup().Count;
		if (setupCount > 1) {
			getCurrentAnimationSetup()[setupCount - 1] = lockTargetPosRot;
		} else {
			getCurrentAnimationSetup().Add(lockTargetPosRot);
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

	/// <summary>
	/// Helper method for getting posRot of current target object
	/// </summary>
	/// <returns></returns>
	public PosRotMapping getPosRotFromCurrentObject() {
		GameObject targetObject = ObjectManager.Instance.getFirstObjectByName(animType.ToString());

		if (targetObject == null) {
			Debug.LogError("Failed to find object: " + animType.ToString());
			return null;
		}

		return new PosRotMapping(targetObject.transform);
	}
}
