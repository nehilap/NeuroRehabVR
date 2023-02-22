using UnityEngine;

public class DirectionalOffset : MonoBehaviour {
	[SerializeField] private Vector3 offset;

	[SerializeField] private CapsuleCollider objectCollider;

	[SerializeField] private bool useWorldPos;

	[SerializeField] private Transform offsetObject;

	private void Awake() {
		if (offsetObject == null) {
			offsetObject = transform.parent;
		}
	}

	void LateUpdate() {
		if (objectCollider != null) {
			objectCollider.center = new Vector3(offsetObject.forward.x * offset.x, objectCollider.center.y, offsetObject.forward.z * offset.z);
		} else {
			if (useWorldPos) {
				transform.position = offsetObject.transform.position + new Vector3(offsetObject.forward.x * offset.x, offset.y, offsetObject.forward.z * offset.z);
			} else {
				transform.localPosition = new Vector3(offsetObject.forward.x * offset.x, transform.localPosition.y, offsetObject.forward.z * offset.z);
			}
		}
	}
}
