using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.InputSystem.XR;
using System.Collections.Generic;
using Enums;

public class CharacterManager : NetworkBehaviour
{
	public static CharacterManager localClient;
	public static CharacterManager activePatient;

	[SyncVar(hook = nameof(changeControllerType))]
	public ControllerType controllerType;

	[SyncVar(hook = nameof(changeHMDType))]
	public HMDType hmdType;
	
	[SyncVar] public bool isFemale;
	[SyncVar] public int avatarNumber;
    [SerializeField] private List<GameObject> avatarMalePrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> avatarFemalePrefabs = new List<GameObject>();

	public List<GameObject> targetPrefabs = new List<GameObject>();

	// Items is array of components that may need to be enabled / activated  only locally
	public GameObject[] itemsToActivate;
	public XRBaseController[] XRControllers;
	public XRBaseControllerInteractor[] interactors;
	[SerializeField] private AvatarWalkingController[] avatarWalkingControllers;
	[SerializeField] private NetworkAvatarWalkingController networkAvatarWalkingController;

	[SerializeField] private GameObject cameraTransform;

	public InputActionManager inputActionManager;

	public GameObject arm;
	public GameObject armFake;

	private List<Transform> interactedObjects;

	[SerializeField]
	private GameObject[] objectsToCull;

	[SerializeField]
	private GameObject[] avatars;

	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();
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

		RoleManager roleManager = GameObject.Find("GameManager").GetComponent<RoleManager>();
		if (roleManager.characterRole == Enums.UserRole.Patient) {
			GameObject therapistMenu = GameObject.Find("TherapistMenu");
			if (therapistMenu != null) {
				TherapistMenuManager therapistMenuManager = therapistMenu.GetComponent<TherapistMenuManager>();
			}
		}

		int LayerCameraCull = LayerMask.NameToLayer("CameraCulled");
        foreach (GameObject gameObject in objectsToCull) {
			gameObject.layer = LayerCameraCull;
		}
	}

	public override void OnStartClient () {
		base.OnStartClient();

		if (isLocalPlayer) {
			localClient = this;
		}
	}

	public override void OnStopClient() {
		base.OnStopClient();

		if ((arm != null && armFake != null) && activePatient != null) {
			activePatient = null;
		}
	}

	void Start() {
		interactedObjects = new List<Transform>();

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
		} else {
			if(cameraTransform.GetComponent<AudioListener>() != null) {
				cameraTransform.GetComponent<AudioListener>().enabled = true;
			}
		}

		if ((arm != null && armFake != null) && activePatient == null) {
			activePatient = this;
		}

		changeHMDType(hmdType, hmdType);
		changeControllerType(controllerType, controllerType);

		// Setting correct avatar, based on which one was chosen in the lobby
		GameObject avatar;
		if (isFemale) {
			avatar = avatarFemalePrefabs[avatarNumber % avatarFemalePrefabs.Count];
		} else {
			avatar = avatarMalePrefabs[avatarNumber % avatarMalePrefabs.Count];
		}
		transform.GetComponent<AvatarModelManager>().changeModel(isFemale, avatar);


		// we disable avatars on Server, pointless calculations
		if (isServer) {
			foreach (GameObject avatarObject in avatars) {
				avatarObject.SetActive(false);
			}
		}

		networkAvatarWalkingController.enabled = true;
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
			if (isServer)
				SetItemAuthority(itemNetIdentity, identity);
			else
				CmdSetItemAuthority(itemNetIdentity, identity);
		}
	}

    private void SetItemAuthority(NetworkIdentity item, NetworkIdentity newPlayerOwner) {
		item.gameObject.GetComponent<NetworkTransform>().syncDirection = SyncDirection.ClientToServer;
		Debug.Log("Granting authority:" + item.netId + " to:" + newPlayerOwner.netId);
		item.RemoveClientAuthority();
        item.AssignClientAuthority(newPlayerOwner.connectionToClient);
    }

    [Command]
    public void CmdSetItemAuthority(NetworkIdentity itemID, NetworkIdentity newPlayerOwner) {
        SetItemAuthority(itemID, newPlayerOwner);
    }

    /* 
	*
	* ITEM RELEASE 
	* CURRENTLY NOT USED DUE TO HOW BUGGY IT WAS 
	*/
	// Instead it's handled in CustomNetworkManager.cs
	/*
	void itemRelease(Transform interactedObject) {
		// if not server, we ask server to release authority
		NetworkIdentity itemNetIdentity = interactedObject.GetComponent<NetworkIdentity>();
		if (!itemNetIdentity.hasAuthority) {
			return;
		}
		if (isServer)
			ReleaseAuthority(itemNetIdentity);
		else
			CmdReleaseAuthority(itemNetIdentity);
	}

	void ReleaseAuthority(NetworkIdentity item) {
		Debug.Log("Releasing authority:" + item.netId);
		item.RemoveClientAuthority();
    }

    [Command]
    public void CmdReleaseAuthority(NetworkIdentity itemID) {
        ReleaseAuthority(itemID);
    }
	*/
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
		if (activePatient == null) {
			return;
		}

		if(isShowcase) {
			activePatient.armFake.GetComponent<AnimationController>().startAnimation();
		} else {
			activePatient.arm.GetComponent<AnimationController>().startAnimation();
		}
	}

	[ClientRpc]
	public void RpcStopActualAnimation() {
		if (activePatient == null) {
			return;
		}

		activePatient.armFake.GetComponent<AnimationController>().stopAnimation();
		activePatient.arm.GetComponent<AnimationController>().stopAnimation();
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
}
