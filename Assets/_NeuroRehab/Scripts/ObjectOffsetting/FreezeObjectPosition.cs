using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeObjectPosition : MonoBehaviour {
	private Vector3 initialPosition;

	void OnEnable() {
		initialPosition = transform.position;
	}

	void LateUpdate() {
		transform.position = initialPosition;
	}
}
