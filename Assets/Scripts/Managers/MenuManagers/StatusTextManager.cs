using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class StatusTextManager : MonoBehaviour {
    void Start() {
        setStatusText();
    }

    public void setStatusText() {
        if(!this.gameObject.scene.isLoaded) return;
        
        GetComponent<TMP_Text>().text = "Platform: '" + Application.platform.ToString() 
            + "', HMD type: '" + XRStatusManager.Instance.hmdType.ToString()
            + "', Device: '" + XRSettings.loadedDeviceName + "'";
    }
}
