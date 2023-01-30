using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.InputSystem.XR;
using System.Collections.Generic;
using Enums;
using Wolf3D.ReadyPlayerMe.AvatarSDK;

public class CharacterManager : NetworkBehaviour
{
	public static CharacterManager localClient;
	public static CharacterManager activePatient;

	[SerializeField] private bool isPatient = false;

	[Header("Spawn sync vars")]
	[SyncVar(hook = nameof(changeControllerType))] public ControllerType controllerType;
	[SyncVar(hook = nameof(changeHMDType))] public HMDType hmdType;
	[SyncVar] public bool isFemale;
	[SyncVar] public int avatarNumber;
	[SyncVar] public float avatarSizeMultiplier;

	[Header("Run sync vars")]
	[SyncVar(hook = nameof(changeArmRestingState))] public bool isArmResting;

	[SerializeField] public GameObject activeAvatarObject;
	[SerializeField] public bool isLeftArmAnimated = false;
	[SerializeField] public ArmAnimationController activeArmAnimationController;

    [SerializeField] private List<GameObject> avatarMalePrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> avatarFemalePrefabs = new List<GameObject>();

	// Items is array of components that may need to be enabled / activated  only locally
	[SerializeField] private GameObject[] itemsToActivate;
	[SerializeField] private XRBaseController[] XRControllers;
	[SerializeField] private XRBaseControllerInteractor[] interactors;
	[SerializeField] private AvatarWalkingController[] avatarWalkingControllers;
	[SerializeField] private NetworkAvatarWalkingController networkAvatarWalkingController;
	[SerializeField] private HeadCollisionManager headCollisionManager;

	[SerializeField] private GameObject cameraTransform;

	[SerializeField] private InputActionManager inputActionManager;

	[SerializeField] private GameObject[] objectsToCull;
	[SerializeField] private GameObject[] avatars;

	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();

		localClient = this;
		Debug.Log("Local Character started");

		// Items is array of components that may need to be enabled / activated  only locally
		for (int i = 0; i < itemsToActivate.Length; i++) {
			itemsToActivate[i].SetActive(true);
		}

		for (int i = 0; i < avatarWalkingControllers.Length; i++) {
			avatarWalkingControllers[i].enabled = true;
		}

		// We find all canvases and set them up to work correctly with the VR camera settings
		GameObject[] menus = FindGameObjectsInLayer(6); // 6 = Canvas
		for (int i = 0; i < menus.Length; i++) {
			Canvas canvas = menus[i].GetComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		}

		// Input Action manager has to be enabled only locally, otherwise it would not work, due to how VR works
		// in other words, only local "player" can have it enabled
		inputActionManager.enabled = true;

		// We look for Device simulator and setup the local player camera transform to camera transform
		GameObject.Find("XR Device Simulator").GetComponent<XRDeviceSimulator>().cameraTransform = cameraTransform.transform;

		// We add listeners on item pick up / release
		// interactors should contain all hands (XRcontrollers and interactors) that are to be used to interact with items
		for (int i = 0; i < interactors.Length; i++) {
        	interactors[i].selectEntered.AddListener(itemPickUp);
			// interactors[i].selectExited.AddListener(itemRelease);
		}
        for (int i = 0; i < XRControllers.Length; i++) {
			XRControllers[i].enableInputTracking = true;
			XRControllers[i].enableInputActions = true;
		}

		if (RoleManager.instance.characterRole == Enums.UserRole.Patient) {
			GameObject therapistMenu = GameObject.Find("TherapistMenu");
			if (therapistMenu != null) {
				TherapistMenuManager therapistMenuManager = therapistMenu.GetComponent<TherapistMenuManager>();
			}
		}

		int LayerCameraCull = LayerMask.NameToLayer("CameraCulled");
        foreach (GameObject gameObject in objectsToCull) {
			gameObject.layer = LayerCameraCull;
		}

		if (RoleManager.instance.characterRole != UserRole.Patient) {
			headCollisionManager.enabled = true;
		}
	}

	public override void OnStopClient() {
		base.OnStopClient();

		if (isPatient && activePatient != null) {
			activePatient = null;
		}
	}

	void Start() {
		// Setting up offset based on HMD type used by client
		changeHMDType(hmdType, hmdType);

		// Setting up controller model
		changeControllerType(controllerType, controllerType);

		// Setting correct avatar, based on which one was chosen in the lobby
		GameObject avatar;
		if (isFemale) {
			avatar = avatarFemalePrefabs[avatarNumber % avatarFemalePrefabs.Count];
		} else {
			avatar = avatarMalePrefabs[avatarNumber % avatarMalePrefabs.Count];
		}

		activeAvatarObject = transform.GetComponent<AvatarModelManager>().changeModel(isFemale, avatar, avatarSizeMultiplier);


		if (isPatient && activePatient == null) {
			activePatient = this;
		}

		if (isPatient) {
			ArmAnimationController[] armAnimationControllers = activeAvatarObject.GetComponents<ArmAnimationController>();
			foreach (ArmAnimationController item in armAnimationControllers) {
				if (this.isLeftArmAnimated == item.isLeft) { // only if both True, or both False
					activeArmAnimationController = item;
				} else {
					item.enabled = false;
				}
			}
		} else {
			foreach (ArmAnimationController item in transform.GetComponents<ArmAnimationController>()) {
				item.enabled = false;
			}
		}

		// we disable avatars on Server, pointless calculations, hogs HW too much
		if (isServer) {
			foreach (GameObject avatarObject in avatars) {
				avatarObject.SetActive(false);
			}
		}

		networkAvatarWalkingController.enabled = true;

		// if non local character prefab is loaded we have to disable components such as camera, etc. otherwise Multiplayer aspect wouldn't work properly 
		if (!isLocalPlayer)	{
			if(cameraTransform.GetComponent<Camera>() != null) {
				cameraTransform.GetComponent<Camera>().enabled = false;
			}
			if(cameraTransform.GetComponents<TrackedPoseDriver>() != null) {
				foreach (TrackedPoseDriver item in cameraTransform.GetComponents<TrackedPoseDriver>())	{
					item.enabled = false;
				}
			}
			for (int i = 0; i < XRControllers.Length; i++) {
				XRControllers[i].enabled = false;
			}
			cameraTransform.GetComponent<HeadCollisionManager>().enabled = false;

			activeAvatarObject.GetComponent<VoiceHandler>().enabled = false;
		} else {
			if(cameraTransform.GetComponent<AudioListener>() != null) {
				cameraTransform.GetComponent<AudioListener>().enabled = true;
			}
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


	/**
	*
	* ITEM PICKUP / RELEASE
	* these are the methods used for granting player an authority over items and then releasing the authority after the item is released
	* this is used for certain VR functions to work as intended in multiplayer space such as grabbing an item
	* 
	*/
	private void itemPickUp(SelectEnterEventArgs args) {
		// we get net identity from current object of character
		NetworkIdentity identity = base.GetComponent<NetworkIdentity>();

		if (identity == null) return;

		if (isOwned) {
			// if not server, we ask server to grant us authority
			NetworkIdentity itemNetIdentity = args.interactableObject.transform.GetComponent<NetworkIdentity>();
			if (!isServer)
				// SetItemAuthority(itemNetIdentity, identity);
			// else
				NetworkCharacterManager.localNetworkClient.CmdSetItemAuthority(itemNetIdentity, identity);
		}
	}


	public void changeControllerType(ControllerType _old, ControllerType _new) {
		// Debug.Log("Change controller called "  +  _new.ToString());
		XRBaseController rightC =  transform.Find("Offset/Camera Offset/RightHand Controller").GetComponent<XRBaseController>();
        XRBaseController leftC =  transform.Find("Offset/Camera Offset/LeftHand Controller").GetComponent<XRBaseController>();

        foreach (GameObject item in HMDInfoManager.instance.controllerPrefabs) {
            if (item.name.Contains(_new.ToString())) {
                if (item.name.Contains("Left")) {
                    leftC.modelPrefab = item.transform;
                }else if (item.name.Contains("Right")) {
                    rightC.modelPrefab = item.transform;
                }
            }
        }

		if (rightC.model != null) {
			rightC.model.gameObject.SetActive(false);
		}

		if (leftC.model != null) {
			leftC.model.gameObject.SetActive(false);
		}

		if (leftC.modelParent != null) {
        	leftC.model = Instantiate(leftC.modelPrefab, leftC.modelParent.transform.position, leftC.modelParent.transform.rotation, leftC.modelParent.transform);
		}
		if (rightC.modelParent != null) {
        	rightC.model = Instantiate(rightC.modelPrefab, rightC.modelParent.transform.position, rightC.modelParent.transform.rotation, rightC.modelParent.transform);
		}
	}

	public void changeHMDType(HMDType _old, HMDType _new) {
		if (_new == HMDType.Other) {
			transform.Find("Offset").position = new Vector3(0f, 0f, 0f);
		}
	}

	private void changeArmRestingState(bool _old, bool _new) {
		if (!isPatient) {
			return;
		}

		activeArmAnimationController.setArmRestPosition();
	}
}
