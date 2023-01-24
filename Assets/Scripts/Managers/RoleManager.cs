using UnityEngine;
using Enums;

// Component holding information about Character such as role etc
// this component is attached to object that is not despawned
public class RoleManager : MonoBehaviour {
    public static RoleManager instance;
    public UserRole characterRole;

    private void Start() {
        if (instance == null) {
            instance = this;
        }
    }

    public void CreateCharacter(UserRole role) {
        characterRole = role;
    }
}
