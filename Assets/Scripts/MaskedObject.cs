using UnityEngine;

public class MaskedObject : MonoBehaviour {
    void Start() {
        GetComponent<SkinnedMeshRenderer>().material.renderQueue = 3002;
    }
}
