using Mirror;
using UnityEngine;
using System.Collections.Generic;
using Wolf3D.ReadyPlayerMe.AvatarSDK;

public class CharacterManager : NetworkBehaviour {
	
	public static CharacterManager localClientInstance { get; private set; }
	public static CharacterManager activePatientInstance { get; private set; }

	[SerializeField] private bool isPatient = false;

	[Header("Spawn sync vars")]
	[SyncVar] public bool isFemale;
	[SyncVar] public int avatarNumber;
	[SyncVar] public float avatarSizeMultiplier;

	[Header("Run sync vars")]
	[SyncVar(hook = nameof(changeArmRestingState))] public bool isArmResting;

	[Header("Active avatar")]
	[SerializeField] public GameObject activeAvatarObject;
	[SerializeField] public ArmAnimationController activeArmAnimationController;
	[SerializeField] public bool isLeftArmAnimated = false;
	[SerializeField] public Transform offsetObject;

	[Header("Avatars prefabs used")]
	[SerializeField] private List<GameObject> avatarMalePrefabs = new List<GameObject>();
	[SerializeField] private List<GameObject> avatarFemalePrefabs = new List<GameObject>();

	[Header("Activated objects based on 'authority'")]
	// Items is array of components that may need to be enabled / activated  only locally
	[SerializeField] private GameObject[] itemsToActivate;
	[SerializeField] private AvatarWalkingController[] avatarWalkingControllers;
	[SerializeField] private NetworkAvatarWalkingController networkAvatarWalkingController;

	[SerializeField] protected GameObject cameraTransform;

	[Header("Camera culling and objects to disable")]
	[SerializeField] private GameObject[] objectsToCull;
	[SerializeField] private GameObject[] avatars;

	[Header("Components On / Off")]
	[SerializeField] private MonoBehaviour[] componentsToDisable;
	[SerializeField] private MonoBehaviour[] componentsToEnable;
	[SerializeField] private MonoBehaviour[] componentsToEnableLocally;

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
		Dictionary<string, GameObject> menus = MenuManager.Instance.getMenuList();
		foreach (var item in menus) {
			if (item.Value.layer == LayerMask.NameToLayer("Canvas")) { // 6 = Canvas
				if (item.Value.TryGetComponent<Canvas>(out Canvas canvas)) {
					canvas.renderMode = RenderMode.WorldSpace;
					canvas.worldCamera = cameraTransform.GetComponent<Camera>();
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

	public void Awake() {
		if (isPatient && activePatientInstance == null) {
			activePatientInstance = this;
		}
	}

	public void Start() {
		// Setting correct avatar, based on which one was chosen in the lobby
		GameObject avatar;
		if (isFemale) {
			avatar = avatarFemalePrefabs[avatarNumber % avatarFemalePrefabs.Count];
		} else {
			avatar = avatarMalePrefabs[avatarNumber % avatarMalePrefabs.Count];
		}

		activeAvatarObject = transform.GetComponent<AvatarModelManager>().changeModel(isFemale, avatar, avatarSizeMultiplier);

		if (offsetObject != null) {
			offsetObject.position *= avatarSizeMultiplier;
		}

		if (isPatient) {
			ArmAnimationController[] armAnimationControllers = activeAvatarObject.GetComponents<ArmAnimationController>();
			foreach (ArmAnimationController armController in armAnimationControllers) {
				if (this.isLeftArmAnimated == armController.isLeft) { // only if both True, or both False
					activeArmAnimationController = armController;

					if (isArmResting) {
						activeArmAnimationController.setArmRestPosition();
					}
				} else {
					armController.enabled = false;
				}
			}
		} else {
			foreach (ArmAnimationController item in transform.GetComponents<ArmAnimationController>()) {
				item.enabled = false;
			}
		}

		// we disable avatars on Server, pointless calculations, hogs HW too much
		if (isServerOnly) {
			foreach (GameObject avatarObject in avatars) {
				avatarObject.SetActive(false);
			}
		}

		networkAvatarWalkingController.enabled = true;

		// if non local character prefab is loaded we have to disable components such as camera, etc. otherwise Multiplayer aspect wouldn't work properly 
		if (!isLocalPlayer)	{
			if(cameraTransform.TryGetComponent<Camera>(out Camera camera)) {
				camera.enabled = false;
			}

			if (activeAvatarObject.TryGetComponent<VoiceHandler>(out VoiceHandler voiceHandler)) {
				voiceHandler.enabled = false;
			}
			
			if(cameraTransform.TryGetComponent<AudioListener>(out AudioListener audioListener)) {
				audioListener.enabled = false;
			}

			foreach (MonoBehaviour component in componentsToDisable) {
				component.enabled = false;	
			}
		} else {
			if(cameraTransform.TryGetComponent<AudioListener>(out AudioListener audioListener)) {
				audioListener.enabled = true;
			}
		}

		foreach (MonoBehaviour component in componentsToEnable) {
			component.enabled = true;	
		}
	}

	// We search through all objects loaded in scene in certain layer
	// ideally shouldn't be used too much when there are too many objects
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

	private void changeArmRestingState(bool _old, bool _new) {
		if (!isPatient || activeArmAnimationController == null) {
			return;
		}

		activeArmAnimationController.setArmRestPosition();
	}
}
