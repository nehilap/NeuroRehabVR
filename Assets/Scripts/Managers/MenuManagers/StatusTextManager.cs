using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class StatusTextManager : MonoBehaviour
{
    void Start() {
        GetComponent<TMP_Text>().text = "HMD type: " + HMDInfoManager.instance.hmdType.ToString() + ", '" + XRSettings.loadedDeviceName + "'";
    }
}
