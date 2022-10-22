using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.InputSystem.XR;

public class CharacterManager : NetworkBehaviour
{
	public GameObject[] items;

	public ActionBasedController[] XRControllers;

	public XRBaseControllerInteractor[] interactors;

	public GameObject head;

	public InputActionManager inputActionManager;

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		Debug.Log("Local Character started");

		// Items is array of components that may need to be enabled / activated  only locally (currently not used)
		for (int i = 0; i < items.Length; i++) {
			items[i].SetActive(true);
		}

		// We find all canvases and set them up to work correctly with the VR camera settings
		GameObject[] menus = FindGameObjectsInLayer(6); // 6 = Canvas
		for (int i = 0; i < menus.Length; i++)
		{
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
	}

	void Start()
	{
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
		} else { // isLocalPlayer == true
			// if we are local player, we call method to setup visibility of the menu in OUR view only
			Debug.Log(GameObject.Find("TherapistMenu"));
			if (GameObject.Find("TherapistMenu") != null) {
				// GameObject.Find("TherapistMenu").GetComponent<RoleBasedVisibility>().setupVisibility();
			}
		}
	}

	// We search through all object loaded in scene in certain layer
	// ideally shouldn't be used too much when there are many objects
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
}
