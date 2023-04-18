using UnityEngine;

/// <summary>
/// Class used to mark item as Mask.
/// </summary>
public class MaskedObject : MonoBehaviour {
	void Start() {
		GetComponent<Renderer>().material.renderQueue = 3002;
	}
}
