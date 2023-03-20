using UnityEngine;

public class SimpleRigManager : MonoBehaviour {
	[SerializeField] private Transform offset;

	void Start() {
		changeHMDOffset();
	}

	/// <summary>
	/// This is used in offline scene for simple rig (this rig does not have CharacterManager component)
	/// </summary>
	public void changeHMDOffset() {
		if (XRStatusManager.Instance.hmdType == Enums.HMDType.Other) {
			offset.position = new Vector3(0f, 0f, 0f);
		}
	}
}
