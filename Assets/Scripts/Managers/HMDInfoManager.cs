using UnityEngine;
using UnityEngine.XR;
using Enums;
using System.Collections.Generic;
using TMPro;

public class HMDInfoManager : MonoBehaviour {

    public TMP_Text statusText;

    public static HMDInfoManager instance;

    // https://forum.unity.com/threads/openxr-is-it-no-longer-possible-to-get-descriptive-device-names.1051493/
    public ControllerType controllerType = ControllerType.Quest2;
    
    public List<GameObject> controllerPrefabs = new List<GameObject>();

    public HMDType hmdType {
        get; private set;
    }

    void Start() {
        hmdType = HMDType.Other;
        if (instance == null) {
            instance = this;
        }

        // NOT WORKING CURRENTLY (MOST LIKELY)
        // seems to only work when using MOCK HMD
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

        if (Application.platform == RuntimePlatform.WindowsPlayer) {
            XRSettings.gameViewRenderMode = GameViewRenderMode.LeftEye;
            hmdType = HMDType.Mock;
        } else if (Application.platform == RuntimePlatform.Android) {
            hmdType = HMDType.Other;
        }

        statusText.text = "HMD type: " + hmdType.ToString() + ", '" + XRSettings.loadedDeviceName + "'";
    }
}
