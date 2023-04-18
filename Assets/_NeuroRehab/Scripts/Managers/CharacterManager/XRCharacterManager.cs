using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.InputSystem.XR;
using Enums;
using Unity.XR.CoreUtils;
using NeuroRehab.Utility;

/// <summary>
/// Custom implementation of 'CharacterManager' for XR client. Used both in traditional and simulated XR/VR.
/// </summary>
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

	[SerializeField] private Transform headCollider;

	private XROrigin xrOrigin;
	private CharacterController characterController;

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

		xrOrigin = gameObject.GetComponent<XROrigin>();
		characterController = gameObject.GetComponent<CharacterController>();

		// Setting up offset based on HMD type used by client
		changeHMDType(hmdType, hmdType);

		// Setting up controller model
		changeControllerType(controllerType, controllerType);

		// if non local character prefab is loaded we have to disable components such as camera, etc. otherwise Multiplayer aspect wouldn't work properly
		if (!isLocalPlayer) {
			if (headCollider.TryGetComponent<FreezeObjectRotation>(out var freezeObjectRotation)) {
				freezeObjectRotation.enabled = false;
			}
			if (cameraObject.GetComponents<TrackedPoseDriver>() != null) {
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

	private void LateUpdate() {
		if (!isLocalPlayer) {
			Vector3 center = xrOrigin.CameraInOriginSpacePos;
			if (headCollider) {
				center = characterController.transform.InverseTransformPoint(headCollider.TransformPoint(headCollider.GetComponent<CapsuleCollider>().center));
			}
			center.y = xrOrigin.CameraInOriginSpaceHeight / 2f + characterController.skinWidth;

			characterController.height = xrOrigin.CameraInOriginSpaceHeight;
			characterController.center = center;
		}
	}

	/**
	*
	* ITEM PICKUP / RELEASE
	* these are the methods used for granting player an authority over items and then releasing the authority after the item is released
	* this is used for certain VR functions to work as intended in multiplayer space such as grabbing an item
	*
	*/
	/// <summary>
	/// Handler method for item pickup. We request authority from Server and we also show arm range if possible
	/// </summary>
	/// <param name="args">Info about object grabbed</param>
	private void itemPickUp(SelectEnterEventArgs args) {
		// we get net identity from current object of character
		NetworkIdentity identity = base.GetComponent<NetworkIdentity>();

		if (identity == null) return;

		if (isOwned) {
			// if not server, we ask server to grant us authority
			NetworkIdentity itemNetIdentity = args.interactableObject.transform.GetComponent<NetworkIdentity>();
			if (!isServer && !itemNetIdentity.isOwned){
				NetworkCharacterManager.localNetworkClientInstance.CmdSetItemAuthority(itemNetIdentity, identity);
			}

			args.interactableObject.transform.transform.TryGetComponent<DragInterface>(out DragInterface dragInterface);
			dragInterface?.OnShowDragRange();
		}
	}

	/// <summary>
	/// Handler method for item release. We simply hide arm range. We don't have to release authority, we can save up some processing this way...
	/// </summary>
	/// <param name="args"></param>
	private void itemRelease(SelectExitEventArgs args) {
		if (isOwned) {
			args.interactableObject.transform.transform.TryGetComponent<DragInterface>(out DragInterface dragInterface);
			dragInterface?.OnHideDragRange();
		}
	}

	/// <summary>
	/// Method to change controller model
	/// </summary>
	/// <param name="_old"></param>
	/// <param name="_new"></param>
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
		}
		if (rightC.modelParent != null) {
			rightC.model = Instantiate(rightC.modelPrefab, rightC.modelParent.transform.position, rightC.modelParent.transform.rotation, rightC.modelParent.transform);
		}
	}

	/// <summary>
	/// Handler method that handles HMD type changes. In case of 'Other' HMD type we reset Offset (this is in case you are using VR headset so that you are not too tall). The offset exists because simulated HMD would otherwise be on the ground.
	/// </summary>
	/// <param name="_old"></param>
	/// <param name="_new"></param>
	public void changeHMDType(HMDType _old, HMDType _new) {
		if (_new == HMDType.Other) {
			transform.Find("Offset").position = new Vector3(0f, 0f, 0f);
		}
	}

	/// <summary>
	/// Handler method for changing active animated arm.
	/// </summary>
	/// <param name="_old"></param>
	/// <param name="_new"></param>
	public override void changeAnimatedArm(bool _old, bool _new) {
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

	/// <summary>
	/// Teleports character to specific location and rotates it to look in the direction of targetPosition transform. LokTarget is ignored on XRCharacterManager.
	/// </summary>
	/// <param name="targetPosition"></param>
	/// <param name="lookTarget"></param>
	public override void teleportCharacter(Transform targetPosition, Transform lookTarget = null) {
		if (targetPosition == null) {
			Debug.LogError("Argument 'targetPosition' cannot be null");
			return;
		}
		if (xrOrigin != null) {
			// We have to turn off character controller, as it stops us trying to teleport object around
			CharacterController cc = gameObject.GetComponent<CharacterController>();
			cc.enabled = false;

			Vector3 targetCameraPos = targetPosition.position;
			targetCameraPos.y += xrOrigin.CameraYOffset;
			xrOrigin.MoveCameraToWorldLocation(targetCameraPos);

			float angleToRotate = targetPosition.rotation.eulerAngles.y - base.cameraObject.transform.rotation.eulerAngles.y;
			xrOrigin.RotateAroundCameraUsingOriginUp(angleToRotate);

			cc.enabled = true;
		} else {
			Debug.LogError("Failed to retrieve XROrigin");
		}
	}
}
