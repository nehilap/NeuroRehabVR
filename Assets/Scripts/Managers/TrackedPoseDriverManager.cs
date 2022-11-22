using UnityEngine;
using UnityEngine.InputSystem.XR;

public class TrackedPoseDriverManager : MonoBehaviour
{
    void Start() {
        // We have to be sure HMDInfoManager is setup before this code runs
        // we can do that in "Edit" -> "Project Settings" -> "Script Execution Order"
        TrackedPoseDriver[] trackedPoseDrivers = GetComponents<TrackedPoseDriver>();

        if (HMDInfoManager.instance.hmdType == Enums.HMDType.Mock || HMDInfoManager.instance.hmdType == Enums.HMDType.Server) {
            for (int i = 0; i < trackedPoseDrivers.Length; i++) {
                trackedPoseDrivers[i].enabled = !trackedPoseDrivers[i].enabled;
            }
        }
    }
}
