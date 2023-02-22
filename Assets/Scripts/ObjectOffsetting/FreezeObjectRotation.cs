using UnityEngine;

public class FreezeObjectRotation : MonoBehaviour {
	private Quaternion initialRotation;

	void OnEnable() {
		initialRotation = transform.rotation;
	}

	// Update is called once per frame
	void LateUpdate() {
		transform.rotation = initialRotation;
	}
}
