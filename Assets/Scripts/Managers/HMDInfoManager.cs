using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Enums;

public class HMDInfoManager : MonoBehaviour {

    public static HMDInfoManager instance;

    public TMP_Text statusText;

    // https://forum.unity.com/threads/openxr-is-it-no-longer-possible-to-get-descriptive-device-names.1051493/
    public ControllerType controllerType = ControllerType.Quest2;
    
    public List<GameObject> controllerPrefabs = new List<GameObject>();

    public HMDType hmdType;

    void Start() {
        if (instance == null) {
            instance = this;
        }
        
        hmdType = HMDType.Other;

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
            // StartCoroutine(HMDInfoManager.instance.startXR());
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
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            // Camera.main.ResetAspect();
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
        if(hmdType == HMDType.Mock){
            XRSettings.gameViewRenderMode = GameViewRenderMode.LeftEye;
        }
        
        // We actually have to increase the resolution scaling, to increase the image queality
        // because 1.0 creates artifacts / jagged lines
        XRSettings.eyeTextureResolutionScale = 1.6f;
    }

}
