using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class StatusTextManager : MonoBehaviour {
    void Start() {
        setStatusText();
    }

    public void setStatusText() {
        GetComponent<TMP_Text>().text = "Platform: '" + Application.platform.ToString() 
            + "', HMD type: '" + XRStatusManager.instance.hmdType.ToString()
            + "', Device: '" + XRSettings.loadedDeviceName + "'";
    }
}
