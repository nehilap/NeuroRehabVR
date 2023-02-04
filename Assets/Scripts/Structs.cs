using Enums;
using Mirror;

namespace Structs {
    // Message used to inform server which prefab to use when spawning a character
    public struct CharacterMessage : NetworkMessage
    {
        public UserRole role;
        public HMDType hmdType;
        public ControllerType controllerType;
        public bool isFemale;
        public int avatarNumber;
        public float sizeMultiplier;
        public bool isXRActive;
    }
}
