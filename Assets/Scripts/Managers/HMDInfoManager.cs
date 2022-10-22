using UnityEngine;
using UnityEngine.XR;
using Enums;

public class HMDInfoManager : MonoBehaviour
{
    public HMDType hmdType
    {
        get; private set;
    }

    void Start()
    {
        // discovers which type of HMD device is being used
        // we can use this to decide whether to spawn simulated character or not
        if(!XRSettings.isDeviceActive) {
            Debug.Log("No HMD discovered");
            hmdType = HMDType.NotFound;
        }else {
            Debug.Log("HMD discovered: " + XRSettings.loadedDeviceName);

            hmdType = HMDType.Other;
            if(XRSettings.loadedDeviceName.Equals("MockHMD Display")) { 
                // if MockHMD, we make the game view render only using one eye
                XRSettings.gameViewRenderMode = GameViewRenderMode.LeftEye;
                hmdType = HMDType.Mock;
            }
        }
    }

}
