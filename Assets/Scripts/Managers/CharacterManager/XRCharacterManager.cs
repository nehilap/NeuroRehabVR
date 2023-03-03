using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.InputSystem.XR;
using Enums;

public class XRCharacterManager : CharacterManager {

	//[Header("Run sync vars")]

	//[Header("Avatars prefabs used")]

	//[Header("Camera culling and objects to disable")]
	[Header("XR Character Manager")]
	[Header("Spawn sync vars")]
	[SyncVar(hook = nameof(changeControllerType))] public ControllerType controllerType;
	[SyncVar(hook = nameof(changeHMDType))] public HMDType hmdType;

	[Header("Activated objects based on 'authority'")]
	// Items is array of components that may need to be enabled / activated  only locally
	[SerializeField] private XRBaseController[] XRControllers;
	[SerializeField] private XRBaseControllerInteractor[] XRInteractors;

	[SerializeField] private InputActionManager inputActionManager;

	[SerializeField] private GameObject xrDeviceSimulator;

	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();

		// Input Action manager has to be enabled only locally, otherwise it would not work, due to how VR works
		// in other words, only local "player" can have it enabled
		inputActionManager.enabled = true;

		// We look for Device simulator and setup the local player camera transform to camera transform
		if (hmdType == HMDType.Mock && XRStatusManager.Instance.isXRActive) {
			GameObject deviceSimulator = Instantiate(xrDeviceSimulator);
			
			deviceSimulator.GetComponent<XRDeviceSimulator>().cameraTransform = cameraObject.transform;
		}

		// We add listeners on item pick up / release
		// interactors should contain all hands (XRcontrollers and interactors) that are to be used to interact with items
		for (int i = 0; i < XRInteractors.Length; i++) {
			XRInteractors[i].selectEntered.AddListener(itemPickUp);
			XRInteractors[i].selectExited.AddListener(itemRelease);
		}
		for (int i = 0; i < XRControllers.Length; i++) {
			XRControllers[i].enableInputTracking = true;
			XRControllers[i].enableInputActions = true;
		}
	}

	public override void OnStopClient() {
		base.OnStopClient();
	}

	public override void Start() {
		base.Start();

		// Setting up offset based on HMD type used by client
		changeHMDType(hmdType, hmdType);

		// Setting up controller model
		changeControllerType(controllerType, controllerType);
	
		// if non local character prefab is loaded we have to disable components such as camera, etc. otherwise Multiplayer aspect wouldn't work properly 
		if (!isLocalPlayer)	{
			if(cameraObject.GetComponents<TrackedPoseDriver>() != null) {
				foreach (TrackedPoseDriver item in cameraObject.GetComponents<TrackedPoseDriver>())	{
					item.enabled = false;
				}
			}
			for (int i = 0; i < XRControllers.Length; i++) {
				XRControllers[i].enabled = false;
			}
			if (cameraObject.TryGetComponent<HeadCollisionManager>(out HeadCollisionManager headCollisionManager)) {
				headCollisionManager.enabled = false;
			}
		}
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
			if (!isServer){
				NetworkCharacterManager.localNetworkClientInstance.CmdSetItemAuthority(itemNetIdentity, identity);
			}

			args.interactableObject.transform.transform.TryGetComponent<DragInterface>(out DragInterface dragInterface);
			dragInterface?.OnShowDragRange();
		}
	}

	private void itemRelease(SelectExitEventArgs args) {
		if (isOwned) {
			args.interactableObject.transform.transform.TryGetComponent<DragInterface>(out DragInterface dragInterface);
			dragInterface?.OnHideDragRange();
		}
	}

	public void changeControllerType(ControllerType _old, ControllerType _new) {
		// Debug.Log("Change controller called " + _new.ToString());

		XRBaseController rightC = null;
		XRBaseController leftC = null;
		foreach (XRBaseController item in XRControllers) {
			if (item.modelPrefab != null) {
				if (item.gameObject.TryGetComponent<XRControllerUtility>(out XRControllerUtility xrControllerUtility)) {
					if (xrControllerUtility.isLeftHandController) {
						leftC = item;
					} else {
						rightC = item;
					}
				}
			}
		}

		if (leftC == null || rightC == null) {
			Debug.LogError("Failed to find controller objects");
			return;
		}

		foreach (GameObject item in XRStatusManager.Instance.controllerPrefabs) {
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
			Outline outline = leftC.model.gameObject.AddComponent(typeof(Outline)) as Outline;
			outline.OutlineMode = Outline.Mode.OutlineHidden;
		}
		if (rightC.modelParent != null) {
			rightC.model = Instantiate(rightC.modelPrefab, rightC.modelParent.transform.position, rightC.modelParent.transform.rotation, rightC.modelParent.transform);
			Outline outline = rightC.model.gameObject.AddComponent(typeof(Outline)) as Outline;
			outline.OutlineMode = Outline.Mode.OutlineHidden;
		}
	}

	public void changeHMDType(HMDType _old, HMDType _new) {
		if (_new == HMDType.Other) {
			transform.Find("Offset").position = new Vector3(0f, 0f, 0f);
		}
	}

	protected override void changeAnimatedArm(bool _old, bool _new) { 
		base.changeAnimatedArm(_old, _new);

		for (int i = 0; i < XRControllers.Length; i++) {
			if (XRControllers[i].TryGetComponent<XRControllerUtility>(out XRControllerUtility xrControllerUtility)) {
				// We turn off controller and ray interactors, based on which arm is being used
				if (base.isLeftArmAnimated == xrControllerUtility.isLeftHandController) { 
					XRControllers[i].gameObject.SetActive(false);
				} else {
					XRControllers[i].gameObject.SetActive(true);
				}
			} else {
				Debug.LogWarning("Failed to identify XRController, most likely missing 'XRControllerUtility'!");
			}
		}
	}
}
