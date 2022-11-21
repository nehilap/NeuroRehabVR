using Enums;
using Mirror;
using UnityEngine;

namespace Structs {
    // Message used to inform server which prefab to use when spawning a character
    public struct CharacterMessage : NetworkMessage
    {
        public UserRole role;
        public HMDType hmdType;
        public ControllerType controllerType;
    }
}
