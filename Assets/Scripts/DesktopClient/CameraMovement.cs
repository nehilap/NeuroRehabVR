using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour {
	
	[SerializeField] private InputActionReference mouseX;
	[SerializeField] private InputActionReference mouseY;
	[SerializeField] private InputActionReference menu;
	[SerializeField] private InputActionReference grab;

	[SerializeField] private Transform player;

	[SerializeField] private float verticalRotation = 0f;
	[SerializeField] private float mouseSensitivityX = 50f;
	[SerializeField] private float mouseSensitivityY = 50f;


	private void OnEnable() {
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnDisable() {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	void LateUpdate() {
		if (MouseManager.Instance.activeTriggers > 0) {
			return;
		}

		float _mouseX =  mouseX.action.ReadValue<float>() * mouseSensitivityX * Time.deltaTime;
		float _mouseY = mouseY.action.ReadValue<float>() * mouseSensitivityY * Time.deltaTime;

		verticalRotation -= _mouseY;
		verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);

		transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
		player.Rotate(Vector3.up * _mouseX);
	}
}
