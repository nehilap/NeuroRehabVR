using UnityEngine;
using Mirror;
using Structs;
using Enums;

// Class overriding default NetworkManager from Mirror
// used for spawning custom character models
public class CustomNetworkManager : NetworkManager {   
	// public bool isServer;
	// usable prefabs for character (first non-simulated, then simulated prefabs)

	[Header("Prefabs for character")]
	[SerializeField] private GameObject therapistXRPrefab;
	[SerializeField] private GameObject patientXRPrefab;
	[SerializeField] private GameObject therapistDesktopPrefab;
	
	// the reason is because NetworkManager (parent class) already starts server if this is server build
	// in case you still need to use Start(), don't forget to call base.Start(); 
	
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
		#if UNITY_SERVER
		XRStatusManager.Instance.stopXR();
		#endif

		base.Start();
	}

	// Called on SERVER only when SERVER starts
	public override void OnStartServer() {
		base.OnStartServer();

		Debug.Log("Server started:" + NetworkManager.singleton.networkAddress);

		
		NetworkServer.RegisterHandler<CharacterMessage>(OnCreateCharacter);
	}

	// Called on CLIENT only when CLIENT connects
	public override void OnClientConnect() {
		base.OnClientConnect();
		
		// you can send the message here
		CharacterMessage characterMessage = new CharacterMessage
		{
			role = RoleManager.Instance.characterRole,
			hmdType = XRStatusManager.Instance.hmdType,
			controllerType = XRStatusManager.Instance.controllerType,
			isFemale = SettingsManager.Instance.avatarSettings.isFemale,
			avatarNumber = SettingsManager.Instance.avatarSettings.avatarNumber,
			sizeMultiplier = SettingsManager.Instance.avatarSettings.sizeMultiplier,
			isXRActive = XRStatusManager.Instance.isXRActive,
		};

		NetworkClient.Send(characterMessage);
	}

	// https://mirror-networking.gitbook.io/docs/guides/gameobjects/custom-character-spawning
	void OnCreateCharacter(NetworkConnectionToClient conn, CharacterMessage message) {
		Debug.Log("New connection requested, Client using: HMD: '" + message.hmdType.ToString() + "'" + ", female: '" + message.isFemale + "', avatarIndex: '" + message.avatarNumber + "', role: '" + message.role + "', XR: '" + message.isXRActive + "'");

		GameObject characterPrefab;
		if (message.role == UserRole.Therapist) {
			if (message.isXRActive) {
				characterPrefab = therapistXRPrefab;
			} else {
				characterPrefab = therapistDesktopPrefab;
			}
		} else if (message.role == UserRole.Patient) {
			characterPrefab = patientXRPrefab;
		} else {
			Debug.LogError("Cannot Instantiate character prefab " + message.role.ToString() + " - not found!!");
			return;
		}
		GameObject newCharacterModel = Instantiate(characterPrefab);
		/*
		int indexToSpawn = -1;
		for (int i = 0; i < characterPrefabs.Count; i++) {
			if (characterPrefabs[i].name == message.role.ToString()) {
				indexToSpawn = i;
				break;
			}
		}
		if (indexToSpawn == -1) {
			Debug.LogError("Cannot Instantiate character prefab " + message.role.ToString() + " - not found!!");
			return;
		}

		GameObject newCharacterModel = Instantiate(characterPrefabs[indexToSpawn]);
		*/
		CharacterManager characterManager = newCharacterModel.GetComponent<CharacterManager>();
		characterManager.isFemale = message.isFemale;
		characterManager.avatarNumber = message.avatarNumber;
		characterManager.avatarSizeMultiplier = message.sizeMultiplier;

		if (message.isXRActive) {
			((XRCharacterManager) characterManager).controllerType = message.controllerType;
			((XRCharacterManager) characterManager).hmdType = message.hmdType;
			// this gets called only on Server, Clients have to change their models themselves anyway
			((XRCharacterManager) characterManager).changeControllerType(message.controllerType, message.controllerType);
		}
		
		Debug.Log("Adding Play for connection: " + conn.connectionId + ", " + newCharacterModel);
		NetworkServer.AddPlayerForConnection(conn, newCharacterModel);
	}

	public override void OnServerDisconnect(NetworkConnectionToClient conn) {
		NetworkIdentity[] ownedObjects = new NetworkIdentity[conn.owned.Count];
		conn.owned.CopyTo(ownedObjects);

		int avatarLayer = LayerMask.NameToLayer("Avatar");
		foreach (NetworkIdentity networkIdentity in ownedObjects) {
			if (networkIdentity.gameObject.layer == avatarLayer) { // Layer 7 = Avatar
				continue;
			}
			networkIdentity.gameObject.GetComponent<NetworkTransform>().syncDirection = SyncDirection.ServerToClient;
			networkIdentity.RemoveClientAuthority();
			Debug.Log("Object with netID '" + networkIdentity.netId + "' released authority.");
		}

		base.OnServerDisconnect(conn);
	}
}
