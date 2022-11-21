using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Enums;
using Structs;
using UnityEngine.XR.Interaction.Toolkit;

// Class overriding default NetworkManager from Mirror
// used for spawning custom character models
public class CustomNetworkManager : NetworkManager
{   
    // public bool isServer;
    // usable prefabs for character (first non-simulated, then simulated prefabs)
    public List<GameObject> characterPrefabs = new List<GameObject>();

    // 2 persistent components holding information
    private RoleManager roleManager;
    
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

        base.Start();
    }
    

    // Fake Start method, since this is not traditional MonoBehaviour and Methods listed below are called on certain events
    private void Setup() {
        roleManager = gameObject.GetComponent<RoleManager>();

        /*characterPrefabs = new Dictionary<string, GameObject>();

        // we load character models from Resources folder (Prefabs folder is inside Resources)
        string[] roles = System.Enum.GetNames(typeof(UserRole));
        for (int i = 0; i < roles.Length; i++) {
            characterPrefabs.Add(roles[i], Resources.Load<GameObject>("Prefabs/CharacterModel/" + roles[i]));
        }*/
    }

    // Called on SERVER only when SERVER starts
    public override void OnStartServer() {
        base.OnStartServer();
        Setup();

        Debug.Log("Server started:" + NetworkManager.singleton.networkAddress);

        
        NetworkServer.RegisterHandler<CharacterMessage>(OnCreateCharacter);
    }

    // Called on CLIENT only when CLIENT connects
    public override void OnClientConnect() {
        base.OnClientConnect();
        Setup();
        
        // you can send the message here
        CharacterMessage characterMessage = new CharacterMessage
        {
            role = roleManager.characterRole,
            hmdType = HMDInfoManager.instance.hmdType,
            controllerType = HMDInfoManager.instance.controllerType
        };

        NetworkClient.Send(characterMessage);
    }

    // https://mirror-networking.gitbook.io/docs/guides/gameobjects/custom-character-spawning
    void OnCreateCharacter(NetworkConnectionToClient conn, CharacterMessage message) {
        Debug.Log(message.hmdType.ToString());

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
        characterManager.changeControllerType(message.controllerType, message.controllerType);

        NetworkServer.AddPlayerForConnection(conn, newCharacterModel);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        NetworkIdentity[] ownedObjects = new NetworkIdentity[conn.owned.Count];
        conn.owned.CopyTo(ownedObjects);
        foreach (NetworkIdentity networkIdentity in ownedObjects) {
            if (networkIdentity.gameObject.layer == 7) {
                continue;
            }
            networkIdentity.gameObject.GetComponent<NetworkTransform>().syncDirection = SyncDirection.ServerToClient;
            networkIdentity.RemoveClientAuthority();
            Debug.Log("Object with netID '" + networkIdentity.netId + "' released authority.");
        }

        base.OnServerDisconnect(conn);
    }
}
