using UnityEngine;
using UnityEngine.InputSystem.XR;

public class TrackedPoseDriverManager : MonoBehaviour {
	public CharacterManager characterManager;
	void Start() {
		bool flag = true;
		if (characterManager != null) {
			if (!characterManager.isLocalPlayer) {
				flag = false;
			}
		}
		// We have to be sure XRStatusManager is setup before this code runs
		// we can do that in "Edit" -> "Project Settings" -> "Script Execution Order"
		if (flag) {
			TrackedPoseDriver[] trackedPoseDrivers = GetComponents<TrackedPoseDriver>();

			if (XRStatusManager.Instance.hmdType == Enums.HMDType.Mock || XRStatusManager.Instance.hmdType == Enums.HMDType.Server) {
				for (int i = 0; i < trackedPoseDrivers.Length; i++) {
					trackedPoseDrivers[i].enabled = !trackedPoseDrivers[i].enabled;
				}
			}
		}
	}
}
