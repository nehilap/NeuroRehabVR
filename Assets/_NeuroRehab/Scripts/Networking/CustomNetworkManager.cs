using UnityEngine;
using Mirror;
using Structs;
using NeuroRehab.Enums;
using System.Collections.Generic;
using VRStandardAssets.Utils;

/// <summary>
/// Class overriding default NetworkManager from Mirror. Used for spawning custom character models and when character disconnects.
/// </summary>
public class CustomNetworkManager : NetworkManager {
	[Header("Prefabs for character")]
	[SerializeField] private GameObject onlineXRPrefab;
	[SerializeField] private GameObject onlineDesktopPrefab;

	/// <summary>
	/// In case you still need to use Start(), don't forget to call 'base.Start();'. The reason is because NetworkManager (parent class) already starts server if this is server build.
	/// </summary>
	public override void Start() {
		string[] args = System.Environment.GetCommandLineArgs ();
		string input = "";
		for (int i = 0; i < args.Length; i++) {
			// Debug.Log ("ARG " + i + ": " + args [i]);
			if (args [i] == "-serverip") {
				input = args [i + 1];
				NetworkManager.singleton.networkAddress = input;
				break;
			}
		}

		if (SettingsManager.Instance.initializedFromFile) {
			networkAddress = SettingsManager.Instance.ipAddress;
		} else {
			SettingsManager.Instance.ipAddress = NetworkManager.singleton.networkAddress;
		}

		#if UNITY_SERVER
		XRSettingsManager.Instance.stopXR();
		#endif

		base.Start();
	}

	// Called on SERVER only when SERVER starts
	public override void OnStartServer() {
		base.OnStartServer();

		Debug.Log($"Server started: {NetworkManager.singleton.networkAddress}");


		NetworkServer.RegisterHandler<CharacterMessage>(OnCreateCharacter);
	}

	/// <summary>
	/// Called on CLIENT only when CLIENT connects
	/// </summary>
	public override void OnClientConnect() {
		base.OnClientConnect();

		// you can send the message here
		CharacterMessage characterMessage = new CharacterMessage {
			role = SettingsManager.Instance.roleSettings.characterRole,
			hmdType = XRSettingsManager.Instance.hmdType,
			controllerType = XRSettingsManager.Instance.controllerType,
			isFemale = SettingsManager.Instance.avatarSettings.isFemale,
			avatarNumber = SettingsManager.Instance.avatarSettings.avatarNumber,
			sizeMultiplier = SettingsManager.Instance.avatarSettings.sizeMultiplier,
			offsetDistance = SettingsManager.Instance.avatarSettings.offsetDistance,
			isXRActive = XRSettingsManager.Instance.isXRActive,
		};

		NetworkClient.Send(characterMessage);
	}

	// https://mirror-networking.gitbook.io/docs/manual/guides/communications/networkmanager-callbacks
	public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) {
		base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);

		List<GameObject> ls = ObjectManager.Instance.getObjectsByName("LoadingScreen");
		foreach (GameObject item in ls) {
			if (!item.activeInHierarchy)
				continue;

			item.GetComponent<Canvas>().enabled = true;

			if (item.TryGetComponent<VRCameraFade>(out VRCameraFade vrCameraFade)) {
				vrCameraFade.FadeOut(false);
			}
		}
	}

	// https://mirror-networking.gitbook.io/docs/manual/guides/communications/networkmanager-callbacks
	public override void OnClientSceneChanged() {
		base.OnClientSceneChanged();

		List<GameObject> ls = ObjectManager.Instance.getObjectsByName("LoadingScreen");
		foreach (GameObject item in ls) {
			if (!item.activeInHierarchy)
				continue;

			item.GetComponent<Canvas>().enabled = false;

			if (item.TryGetComponent<VRCameraFade>(out VRCameraFade vrCameraFade)) {
				vrCameraFade.FadeIn(false);
			}
		}
	}

	/// <summary>
	/// Custom Character spawner. We use CharacterMessage to determine what data to use. Refer to https://mirror-networking.gitbook.io/docs/guides/gameobjects/custom-character-spawning for more information.
	/// </summary>
	/// <param name="conn"></param>
	/// <param name="message"></param>
	void OnCreateCharacter(NetworkConnectionToClient conn, CharacterMessage message) {
		Debug.Log($"New connection requested, Client using: HMD: '{message.hmdType.ToString()}', female: '{message.isFemale}', avatarIndex: '{message.avatarNumber}', role: '{message.role}', XR: '{message.isXRActive}'");

		GameObject characterPrefab;
		if (message.role == UserRole.Therapist || message.role == UserRole.Patient) {
			if (message.isXRActive) {
				characterPrefab = onlineXRPrefab;
			} else {
				characterPrefab = onlineDesktopPrefab;
			}
		} else {
			Debug.LogError($"Cannot Instantiate character prefab '{message.role.ToString()}'- not found!!");
			return;
		}
		GameObject newCharacterModel = Instantiate(characterPrefab);

		CharacterManager characterManager = newCharacterModel.GetComponent<CharacterManager>();
		characterManager.isFemale = message.isFemale;
		characterManager.avatarNumber = message.avatarNumber;
		characterManager.avatarSizeMultiplier = message.sizeMultiplier;
		characterManager.avatarOffsetDistance = message.offsetDistance;
		characterManager.isPatient = (message.role == UserRole.Patient);
		// characterManager.isLeftArmAnimated = message.isLeftArmAnimated;

		if (message.isXRActive) {
			((XRCharacterManager) characterManager).controllerType = message.controllerType;
			((XRCharacterManager) characterManager).hmdType = message.hmdType;

			// this gets called only on Server, Clients have to change their models themselves anyway
			((XRCharacterManager) characterManager).changeControllerType(message.controllerType, message.controllerType);
		}

		Debug.Log($"New connection: '{conn.connectionId}'");
		NetworkServer.AddPlayerForConnection(conn, newCharacterModel);
	}

	/// <summary>
	/// When client disconnects, we clear all client authorities besides Avatar, because otherwise all objects that he has authority over would be destroyed
	/// </summary>
	/// <param name="conn"></param>
	public override void OnServerDisconnect(NetworkConnectionToClient conn) {
		NetworkIdentity[] ownedObjects = new NetworkIdentity[conn.owned.Count];
		conn.owned.CopyTo(ownedObjects);

		int avatarLayer = LayerMask.NameToLayer("Avatar");
		foreach (NetworkIdentity objectIdentity in ownedObjects) {
			if (objectIdentity.gameObject.layer == avatarLayer) { // Layer 7 = Avatar
				continue;
			}

			if (objectIdentity.TryGetComponent<TargetDraggable>(out TargetDraggable targetDraggable)) {
				if (targetDraggable.itemPickedUp) { // this should not happen, but just in case
					targetDraggable.enableDragWrapper();
				}
			}
			objectIdentity.gameObject.GetComponent<NetworkTransform>().syncDirection = SyncDirection.ServerToClient;
			objectIdentity.RemoveClientAuthority();
			Debug.Log($"USER'{conn.identity.netId}' - object with netID '{objectIdentity.netId}' released authority.");
		}

		Debug.Log($"USER'{conn.identity.netId}' - disconnected!");
		base.OnServerDisconnect(conn);
	}
}
