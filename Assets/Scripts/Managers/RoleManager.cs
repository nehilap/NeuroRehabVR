using UnityEngine;
using Enums;

// Component holding information about Character such as role etc
// this component is attached to object that is not despawned
public class RoleManager : MonoBehaviour
{
    public UserRole characterRole;

    public void CreateCharacter(UserRole role)
    {
        characterRole = role;
    }

    // Client-side custom character spawning
    /*
    [Command(requiresAuthority = false)]
    public void CmdCreateCharacter(Role role, NetworkConnectionToClient sender = null)
    {
        int indexIndent = 0;
        if(hmdInfoManager.hmdType == HMDType.Mock) {
            indexIndent = System.Enum.GetNames(typeof(Role)).Length;
        }
        GameObject characterInstance = Instantiate(characters[(int) role + indexIndent]);
        NetworkServer.Spawn(characterInstance, sender);
    }
    */
}
