using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Enums;
using Structs;

// Class overriding default NetworkManager from Mirror
// used for spawning custom character models
public class CustomNetworkManager : NetworkManager
{   
    // usable prefabs for character (first non-simulated, then simulated prefabs)
    public Dictionary<string, GameObject> characterPrefabs;

    // 2 persistent components holding information
    private HMDInfoManager hmdInfoManager;
    private RoleManager characterManager;
    
    // Fake Start method, since this is not traditional MonoBehaviour and Methods listed below are called on certain events
    private void Setup() {
        hmdInfoManager = gameObject.GetComponent<HMDInfoManager>();
        characterManager = gameObject.GetComponent<RoleManager>();

        characterPrefabs = new Dictionary<string, GameObject>();

        // we load character models from Resources folder (Prefabs folder is inside Resources)
        string[] roles = System.Enum.GetNames(typeof(UserRole));
        for (int i = 0; i < roles.Length; i++) {
            characterPrefabs.Add(roles[i], Resources.Load<GameObject>("Prefabs/CharacterModel/Normal/" + roles[i]));
        }
        for (int i = 0; i < roles.Length; i++) {
            characterPrefabs.Add(roles[i]+ "Simulated", Resources.Load<GameObject>("Prefabs/CharacterModel/Simulated/" + roles[i] + "Simulated"));
        }
    }

    // Called on SERVER only when SERVER starts
    public override void OnStartServer()
    {
        base.OnStartServer();
        Setup();

        NetworkServer.RegisterHandler<CharacterMessage>(OnCreateCharacter);
    }

    // Called on CLIENT only when CLIENT connects
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Setup();
        
        // you can send the message here
        CharacterMessage characterMessage = new CharacterMessage
        {
            role = characterManager.characterRole,
            hmdType = hmdInfoManager.hmdType
        };

        NetworkClient.Send(characterMessage);
    }

    // https://mirror-networking.gitbook.io/docs/guides/gameobjects/custom-character-spawning
    void OnCreateCharacter(NetworkConnectionToClient conn, CharacterMessage message)
    {
        string hmdPostfix = "";
        Debug.Log(hmdInfoManager.hmdType.ToString());
        if(hmdInfoManager.hmdType == HMDType.Mock) {
            hmdPostfix = "Simulated";
        }

        GameObject gameobject = Instantiate(characterPrefabs[message.role + hmdPostfix]);
        NetworkServer.AddPlayerForConnection(conn, gameobject);
    }
}
