using Enums;
using Mirror;

namespace Structs {
	/// <summary>
	/// Structure used as message to inform server what settings to apply to User Prefab when spawning a character
	/// </summary>
	public struct CharacterMessage : NetworkMessage
	{
		public UserRole role;
		public HMDType hmdType;
		public ControllerType controllerType;
		public bool isFemale;
		public int avatarNumber;
		public float sizeMultiplier;
		public float offsetDistance;
		public bool isXRActive;
	}
}
