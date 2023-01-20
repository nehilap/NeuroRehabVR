using UnityEngine;

public class FreezeObject : MonoBehaviour {
    private Quaternion initialRotation;

    void Start() {
        initialRotation = transform.rotation;
    }

    // Update is called once per frame
    void LateUpdate() {
        transform.rotation = initialRotation;
    }
}
