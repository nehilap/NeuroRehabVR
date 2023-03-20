// namespace with all enums used
namespace Enums {

	// Role enum
	public enum UserRole {Therapist = 0, Patient = 1};

	// Type of HMD discovered by code
	public enum HMDType {Mock, Other, NotFound, Server};

	// https://forum.unity.com/threads/detailed-xr-inputdevice-names.720614/
	public enum ControllerType {DaydreamVR = 0, HPG2Reverb = 1, HTCVive = 2, Quest2 = 3, Touch = 4, ValveIndex = 5, ViveFocusPlus = 6};

	public enum AnimationType {Off = -1, Cube = 0, Cup = 1, Key = 2, Block = 3};

	public enum AnimationState {Playing, Stopped};

	public enum AnimationPart {Arm, Hand, Moving};
}
