using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class StatusTextManager : MonoBehaviour
{
    void Start() {
        GetComponent<TMP_Text>().text = "Platform: '" + Application.platform.ToString() 
            + "', HMD type: '" + HMDInfoManager.instance.hmdType.ToString()
            + "', Device: '" + XRSettings.loadedDeviceName + "'";
    }
}
