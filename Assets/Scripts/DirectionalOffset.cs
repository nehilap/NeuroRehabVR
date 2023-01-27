using UnityEngine;

public class DirectionalOffset : MonoBehaviour {
    [SerializeField] private Vector3 offset;

    [SerializeField] private CapsuleCollider objectCollider;

    void LateUpdate() {
        if (objectCollider != null) {
            objectCollider.center = new Vector3(transform.parent.forward.x * offset.x, objectCollider.center.y, transform.parent.forward.z * offset.z);
        } else {
            transform.localPosition = new Vector3(transform.parent.forward.x * offset.x, 0, transform.parent.forward.z * offset.z);
        }
    }
}
