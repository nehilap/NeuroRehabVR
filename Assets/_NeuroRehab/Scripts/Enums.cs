// namespace with all enums used
namespace Enums {

	/// <summary>
	/// Role enum, can be extended for more Roles.
	/// </summary>
	public enum UserRole {Therapist = 0, Patient = 1};

	/// <summary>
	/// Type of HMD discovered by code.
	/// </summary>
	public enum HMDType {Mock, Other, NotFound, Server};

	/// <summary>
	/// Used when changing controller model in XR.
	/// </summary>
	public enum ControllerType {DaydreamVR = 0, HPG2Reverb = 1, HTCVive = 2, Quest2 = 3, Touch = 4, ValveIndex = 5, ViveFocusPlus = 6};

	/// <summary>
	/// Patient Arm animation types.
	/// </summary>
	public enum AnimationType {Off = -1, Block = 0, Cube = 1, Cup = 2, Key = 3};

	/// <summary>
	/// Animation States.
	/// </summary>
	public enum AnimationState {Playing, Stopped};

	/// <summary>
	/// Animation Part being currently animated. Not used since switching to Coroutines.
	/// </summary>
	public enum AnimationPart {Arm, Hand, Moving};

	/// <summary>
	/// Message Types used for differentiating what color background to use.
	/// </summary>
	public enum MessageType {OK, WARNING, NORMAL};
}
