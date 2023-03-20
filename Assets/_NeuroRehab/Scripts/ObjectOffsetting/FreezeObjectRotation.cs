using UnityEngine;

public class FreezeObjectRotation : MonoBehaviour {
	private Quaternion initialRotation;

	void OnEnable() {
		initialRotation = transform.rotation;
	}

	void LateUpdate() {
		transform.rotation = initialRotation;
	}
}
