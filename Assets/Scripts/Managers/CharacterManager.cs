using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.InputSystem.XR;

public class CharacterManager : NetworkBehaviour
{
	public static CharacterManager localCharacter;

	public static CharacterManager localPatient;

	public GameObject[] items;

	public ActionBasedController[] XRControllers;

	public XRBaseControllerInteractor[] interactors;

	public GameObject head;

	public InputActionManager inputActionManager;

	public GameObject arm;
	public GameObject armFake;


	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();
		Debug.Log("Local Character started");

		// Items is array of components that may need to be enabled / activated  only locally (currently not used)
		for (int i = 0; i < items.Length; i++) {
			items[i].SetActive(true);
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
		GameObject.Find("XR Device Simulator").GetComponent<XRDeviceSimulator>().cameraTransform = head.transform;

		// We add listeners on item pick up / release
		// interactors should contain all hands (controllers) that are to be used to interact with items
		for (int i = 0; i < interactors.Length; i++) {
        	interactors[i].selectEntered.AddListener(itemPickUp);
			interactors[i].selectExited.AddListener(itemRelease);
		}
        /*
		Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.worldCamera = mainCam;
		canvas.planeDistance = 1;
        */

		RoleManager roleManager = GameObject.Find("GameManager").GetComponent<RoleManager>();
		if (roleManager.characterRole == Enums.UserRole.Patient) {
			GameObject therapistMenu = GameObject.Find("TherapistMenu");
			if (therapistMenu != null) {
				TherapistMenuManager therapistMenuManager = therapistMenu.GetComponent<TherapistMenuManager>();
			}
		}
	}

	public override void OnStartClient () {
            if (isLocalPlayer) {
                localCharacter = this;
            }
        }


	void Start() {
		// if non local character prefab is loaded we have to disable components such as camera, etc. otherwise Multiplayer aspect wouldn't work properly 
		if (!isLocalPlayer)	{
			if(head.GetComponent<Camera>() != null) {
				head.GetComponent<Camera>().enabled = false;
			}
			if(head.GetComponent<TrackedPoseDriver>() != null) {
				head.GetComponent<TrackedPoseDriver>().enabled = false;
			}
			if(head.GetComponent<AudioListener>() != null) {
				head.GetComponent<AudioListener>().enabled = false;
			}

			for (int i = 0; i < XRControllers.Length; i++) {
				XRControllers[i].enabled = false;
			}
		}

		if ((arm != null && armFake != null) && localPatient == null) {
			localPatient = this;
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
	void itemPickUp(SelectEnterEventArgs args) {
		
		// we get net identity from current object of character
		NetworkIdentity identity = base.GetComponent<NetworkIdentity>();
		if (hasAuthority) {
			// if not server, we ask server to grant us authority
			NetworkIdentity itemNetIdentity = args.interactableObject.transform.GetComponent<NetworkIdentity>();
			if (isServer)
				SetItemAuthority(itemNetIdentity, identity);
			else
				CmdSetItemAuthority(itemNetIdentity, identity);
		}
	}

	void itemRelease(SelectExitEventArgs args) {
		if (hasAuthority) {
			// if not server, we ask server to release authority
			NetworkIdentity itemNetIdentity = args.interactableObject.transform.GetComponent<NetworkIdentity>();
			if (isServer)
				ReleaseAuthority(itemNetIdentity);
			else
				CmdReleaseAuthority(itemNetIdentity);
		}
	}
 
    void SetItemAuthority(NetworkIdentity item, NetworkIdentity newPlayerOwner) {
		Debug.Log("Granting authority:" + item.netId + " to:" + newPlayerOwner.netId);
		item.RemoveClientAuthority();
        item.AssignClientAuthority(newPlayerOwner.connectionToClient);
    }

    [Command]
    public void CmdSetItemAuthority(NetworkIdentity itemID, NetworkIdentity newPlayerOwner) {
        SetItemAuthority(itemID, newPlayerOwner);
    }

	void ReleaseAuthority(NetworkIdentity item) {
		Debug.Log("Releasing authority:" + item.netId);
		item.RemoveClientAuthority();
    }

    [Command]
    public void CmdReleaseAuthority(NetworkIdentity itemID) {
        ReleaseAuthority(itemID);
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

	[ClientRpc]
	public void RpcStartActualAnimation(bool isShowcase) {
		if (localPatient == null) {
			return;
		}

		if(isShowcase) {
			localPatient.armFake.GetComponent<AnimationController>().startAnimation();
		} else {
			localPatient.arm.GetComponent<AnimationController>().startAnimation();
		}
	}
/*
	[TargetRpc]
	public void TargetStartActualAnimation(NetworkConnection target, bool isShowcase) {
		if(isShowcase) {
			localCharacter.armFake.GetComponent<AnimationController>().startAnimation();
		} else {
			localCharacter.arm.GetComponent<AnimationController>().startAnimation();
		}
	}
*/
	[ClientRpc]
	public void RpcStopActualAnimation() {
		if (localPatient == null) {
			return;
		}

		localPatient.armFake.GetComponent<AnimationController>().stopAnimation();
		localPatient.arm.GetComponent<AnimationController>().stopAnimation();
	}
}
