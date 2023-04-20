using Mirror;
using UnityEngine;
using System.Collections.Generic;
using Wolf3D.ReadyPlayerMe.AvatarSDK;

/// <summary>
/// Abstract parent class for Multiplayer Character manager. Setups starting components - enables/disables components that would otherwise create conflicts in the scene.
/// </summary>
public abstract class CharacterManager : NetworkBehaviour {

	public static CharacterManager localClientInstance { get; private set; }
	public static CharacterManager activePatientInstance { get; private set; }

	[Header("Spawn sync vars")]
	[SyncVar] public bool isPatient;
	[SyncVar] public bool isFemale;
	[SyncVar] public int avatarNumber;
	[SyncVar] public float avatarSizeMultiplier;
	[SyncVar] public float avatarOffsetDistance;

	[Header("Run sync vars")]
	[SyncVar(hook = nameof(changeAnimatedArm))] public bool isLeftArmAnimated = false;
	[SyncVar(hook = nameof(changeArmRestingState))] public bool isArmResting;

	[Header("Active avatar")]
	[SerializeField] public GameObject activeAvatarObject;
	[SerializeField] public ArmAnimationController activeArmAnimationController;
	[SerializeField] public Transform offsetObject;
	[SerializeField] public GameObject cameraObject;

	[Header("Avatars prefabs used")]
	[SerializeField] private List<GameObject> avatarMalePrefabs = new List<GameObject>();
	[SerializeField] private List<GameObject> avatarFemalePrefabs = new List<GameObject>();

	[Header("Activated objects based on 'authority'")]
	// Items is array of components that may need to be enabled / activated  only locally
	[SerializeField] private GameObject[] itemsToActivate;
	[SerializeField] private AvatarWalkingController[] avatarWalkingControllers;
	[SerializeField] private NetworkAvatarWalkingController networkAvatarWalkingController;

	[Header("Camera culling and objects to disable")]
	[SerializeField] private GameObject[] objectsToCull;
	[SerializeField] private GameObject[] avatars;

	[Header("Components On / Off")]
	[SerializeField] private MonoBehaviour[] componentsToDisable;
	[SerializeField] private MonoBehaviour[] componentsToEnable;
	[SerializeField] private MonoBehaviour[] componentsToEnableLocally;

	[SerializeField] private GameObject activeArmRangeMarker {get; set;}

	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();

		localClientInstance = this;
		Debug.Log("Local Character started");

		// Items is array of components that may need to be enabled / activated  only locally
		for (int i = 0; i < itemsToActivate.Length; i++) {
			itemsToActivate[i].SetActive(true);
		}

		for (int i = 0; i < avatarWalkingControllers.Length; i++) {
			avatarWalkingControllers[i].enabled = true;
		}

		// We find all canvases and set them up to work correctly with the VR camera settings
		Dictionary<string, List<GameObject>> menus = ObjectManager.Instance.getObjectList();
		foreach (var list in menus) {
			foreach (GameObject item in list.Value) {
				if (item.layer == LayerMask.NameToLayer("Canvas")) { // 6 = Canvas
					if (item.TryGetComponent<Canvas>(out Canvas canvas)) {
						canvas.renderMode = RenderMode.WorldSpace;
						canvas.worldCamera = cameraObject.GetComponent<Camera>();
					}
				}
			}
		}

		int LayerCameraCull = LayerMask.NameToLayer("CameraCulled");
		foreach (GameObject gameObject in objectsToCull) {
			gameObject.layer = LayerCameraCull;
		}

		foreach (MonoBehaviour component in componentsToEnableLocally) {
			component.enabled = true;
		}
	}

	public override void OnStopClient() {
		base.OnStopClient();

		if (isPatient && activePatientInstance != null) {
			activePatientInstance = null;
		}
	}

	public virtual void Start() {
		if (isPatient && activePatientInstance == null) {
			activePatientInstance = this;
		}

		// Setting correct avatar, based on which one was chosen in the lobby
		GameObject avatar;
		if (isFemale) {
			avatar = avatarFemalePrefabs[avatarNumber % avatarFemalePrefabs.Count];
		} else {
			avatar = avatarMalePrefabs[avatarNumber % avatarMalePrefabs.Count];
		}

		activeAvatarObject = transform.GetComponent<AvatarModelManager>().changeModel(isFemale, avatar, avatarSizeMultiplier, avatarOffsetDistance);

		if (activeAvatarObject.TryGetComponent<AvatarController>(out AvatarController avatarController)) {
			if (offsetObject != null) {
				offsetObject.position *= avatarSizeMultiplier / avatarController.calculateStandardizedSizeMultiplier();
			}
		}

		if (isPatient) {
			changeAnimatedArm(false, isLeftArmAnimated);
		} else {
			foreach (ArmAnimationController item in transform.GetComponents<ArmAnimationController>()) {
				item.enabled = false;
			}
		}

		networkAvatarWalkingController.enabled = true;
		/*
		// Network Animators from Mirror do not work for Female avatars (dunno why)
		if (isFemale) {
			maleNetworkAnimator.enabled = false;
			femaleNetworkAnimator.enabled = true;
		} else {
			maleNetworkAnimator.enabled = true;
			femaleNetworkAnimator.enabled = false;
		}
		*/

		// if non local character prefab is loaded we have to disable components such as camera, etc. otherwise Multiplayer aspect wouldn't work properly
		if (!isLocalPlayer) {
			if(cameraObject.TryGetComponent<Camera>(out Camera camera)) {
				camera.enabled = false;
			}

			if (activeAvatarObject.TryGetComponent<VoiceHandler>(out VoiceHandler voiceHandler)) {
				voiceHandler.enabled = false;
			}

			if(this.cameraObject.TryGetComponent<AudioListener>(out AudioListener audioListener)) {
				audioListener.enabled = false;
			}

			foreach (MonoBehaviour component in componentsToDisable) {
				component.enabled = false;
			}
		} else {
			if(cameraObject.TryGetComponent<AudioListener>(out AudioListener audioListener)) {
				audioListener.enabled = true;
			}
		}

		foreach (MonoBehaviour component in componentsToEnable) {
			component.enabled = true;
		}
	}

	/// <summary>
	/// We search through all objects loaded in scene in certain laye. Ideally shouldn't be used too much when there are too many objects
	/// </summary>
	/// <param name="layer"></param>
	/// <returns>List of Gameobjects in layer</returns>
	GameObject[] FindGameObjectsInLayer(int layer) {
		var goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
		var goList = new System.Collections.Generic.List<GameObject>();
		for (int i = 0; i < goArray.Length; i++) {
			if (goArray[i].layer == layer) {
				goList.Add(goArray[i]);
			}
		}
		if (goList.Count == 0) {
			return new GameObject[0];
		}
		return goList.ToArray();
	}

	/// <summary>
	/// Handler triggered by SyncVar used to change rest state of active arm
	/// </summary>
	/// <param name="_old"></param>
	/// <param name="_new"></param>
	protected void changeArmRestingState(bool _old, bool _new) {
		if (!isPatient || activeArmAnimationController == null) {
			return;
		}

		activeArmAnimationController.setArmRestPosition(_new);
	}

	/// <summary>
	/// Method used to change active animated arm. First we change active arm variable. If arm is in resting position we make sure it stays that way.
	/// </summary>
	/// <param name="_old"></param>
	/// <param name="_new"></param>
	public virtual void changeAnimatedArm(bool _old, bool _new) {
		if (activeAvatarObject == null) {
			Debug.LogWarning("Missing Active Avatar Object, cannot change animated arm");
			return;
		}

		ArmAnimationController[] armAnimationControllers = activeAvatarObject.GetComponents<ArmAnimationController>();
		foreach (ArmAnimationController armAnimationController in armAnimationControllers) {
			armAnimationController.initElements();
			if (this.isLeftArmAnimated == armAnimationController.isLeft) { // only if both True, or both False
				armAnimationController.enabled = true;
				activeArmAnimationController = armAnimationController;
			} else {
				armAnimationController.enabled = false;
				if (isArmResting) {
					armAnimationController.setArmRestPosition(false);
				}
			}
		}

		if (activeArmAnimationController == null) {
			Debug.LogError("Failed to find correct Arm animation controller for Patient");
			return;
		} else {
			if (activeAvatarObject.TryGetComponent<AvatarController>(out AvatarController avatarController)) {
				if (this.isLeftArmAnimated) {
					avatarController.leftHand.followVRTarget = false;
					activeArmRangeMarker = avatarController.leftArmRangeMarker;
					avatarController.rightHand.followVRTarget = true;
				} else {
					avatarController.rightHand.followVRTarget = false;
					activeArmRangeMarker = avatarController.rightArmRangeMarker;
					avatarController.leftHand.followVRTarget = true;
				}
				avatarController.resetHandIKTargets();
			}
		}

		if (isArmResting) {
			activeArmAnimationController.setArmRestPosition(true);
		}
	}

	public void showArmRangeMarker() {
		MeshRenderer meshRenderer = activeArmRangeMarker.GetComponent<MeshRenderer>();

		meshRenderer.enabled = true;
	}

	public void hideArmRangeMarker() {
		MeshRenderer meshRenderer = activeArmRangeMarker.GetComponent<MeshRenderer>();

		meshRenderer.enabled = false;
	}

	public abstract void teleportCharacter(Transform targetPosition, Transform lookTarget = null);
}
