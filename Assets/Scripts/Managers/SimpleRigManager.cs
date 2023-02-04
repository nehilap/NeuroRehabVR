using UnityEngine;

public class SimpleRigManager : MonoBehaviour {
	[SerializeField] private Transform offset;

    void Start() {
        changeHMDOffset();
    }

    public void changeHMDOffset() {
		if (XRStatusManager.instance.hmdType == Enums.HMDType.Other) {
			offset.position = new Vector3(0f, 0f, 0f);
		}
	}
}
