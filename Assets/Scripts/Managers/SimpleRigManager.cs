using UnityEngine;

public class SimpleRigManager : MonoBehaviour
{
    void Start() {
        changeHMDOffset();
    }

    public void changeHMDOffset() {
		if (HMDInfoManager.instance.hmdType == Enums.HMDType.Other) {
			transform.Find("Offset").transform.position = new Vector3(0f, 0f, 0f);
		}
	}
}
