using UnityEngine;
using UnityEngine.XR;
using Enums;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.XR.Management;

public class HMDInfoManager : MonoBehaviour {

    public TMP_Text statusText;

    public static HMDInfoManager instance;

    // https://forum.unity.com/threads/openxr-is-it-no-longer-possible-to-get-descriptive-device-names.1051493/
    public ControllerType controllerType = ControllerType.Quest2;
    
    public List<GameObject> controllerPrefabs = new List<GameObject>();

    public HMDType hmdType;

    void Start() {
        hmdType = HMDType.Other;
        if (instance == null) {
            instance = this;
        }

        // NOT WORKING CURRENTLY (MOST LIKELY)
        // seems to only work when using MOCK HMD
        // discovers which type of HMD device is being used
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

        // Debug.Log(Application.platform.ToString());
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
            hmdType = HMDType.Mock;
        } else if (Application.platform == RuntimePlatform.Android) {
            hmdType = HMDType.Other;
        } else {
            hmdType = HMDType.Server;
        }

        setXRSettings();
    }

    void OnApplicationQuit() {
        stopXR();
    }

    public void stopXR() {
        if (XRGeneralSettings.Instance.Manager.isInitializationComplete) {
            Debug.Log("Stopping XR...");
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            Camera.main.ResetAspect();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }

    public IEnumerator startXR() {
        if (XRGeneralSettings.Instance.Manager.activeLoader == null) {
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
            if (XRGeneralSettings.Instance.Manager.activeLoader == null) {
                Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
            } else {
                Debug.Log("Starting XR...");
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                
                yield return null;
            }
        }

        setXRSettings();
    }
    public void setXRSettings () {
        XRSettings.gameViewRenderMode = GameViewRenderMode.LeftEye;
        
        // We actually have to increase the resolution scaling, to increase the image queality
        // because 1.0 creates artifacts / jagged lines
        XRSettings.eyeTextureResolutionScale = 1.5f;
    }

}
