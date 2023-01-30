using UnityEngine;

public class MaskedObject : MonoBehaviour {
    void Start() {
        GetComponent<Renderer>().material.renderQueue = 3002;
    }
}
