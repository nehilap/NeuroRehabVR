using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Enums;
using Structs;

// Class overriding default NetworkManager from Mirror
// used for spawning custom character models
public class CustomNetworkManager : NetworkManager
{   
    // public bool isServer;
    // usable prefabs for character (first non-simulated, then simulated prefabs)
    [SerializeField]
    private List<GameObject> characterPrefabs = new List<GameObject>();
    
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
        HMDInfoManager.instance.stopXR();
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
            role = RoleManager.instance.characterRole,
            hmdType = HMDInfoManager.instance.hmdType,
            controllerType = HMDInfoManager.instance.controllerType,
            isFemale = SettingsManager.instance.avatarSettings.isFemale,
            avatarNumber = SettingsManager.instance.avatarSettings.avatarNumber
        };

        NetworkClient.Send(characterMessage);
    }

    // https://mirror-networking.gitbook.io/docs/guides/gameobjects/custom-character-spawning
    void OnCreateCharacter(NetworkConnectionToClient conn, CharacterMessage message) {
        Debug.Log("New connection requested, Client using: '" + message.hmdType.ToString() + "'" +
            ", '" + message.isFemale + "', '" + message.avatarNumber + "'");

        int indexToSpawn = -1;
        for (int i = 0; i < characterPrefabs.Count; i++) {
            if (characterPrefabs[i].name == message.role.ToString()) {
                indexToSpawn = i;
                break;
            }
        }
        if (indexToSpawn == -1) {
            Debug.LogError("Cannot Instantiate character prefab - not found!!");
            return;
        }

        GameObject newCharacterModel = Instantiate(characterPrefabs[indexToSpawn]);
        CharacterManager characterManager = newCharacterModel.GetComponent<CharacterManager>();
        characterManager.controllerType = message.controllerType;
        characterManager.hmdType = message.hmdType;
        characterManager.changeControllerType(message.controllerType, message.controllerType);

        if (message.role == UserRole.Therapist) {
            characterManager.isFemale = message.isFemale;
            characterManager.avatarNumber = message.avatarNumber;
        }
        
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
