using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

public class MouseClick : MonoBehaviour {
	[SerializeField] private InputActionReference mouseClick;

	[SerializeField] private Camera mainCamera;

	[SerializeField] private LayerMask layersToIgnore;

	[SerializeField][Range(0.1f, 50f)] private float rayLength = 10f;

	private void Awake() {
		if (mainCamera == null) {
			mainCamera = Camera.current;
		}
	}

	private void OnEnable() {
		mouseClick.action.performed += mouseTargetClicked;
	}

	private void OnDisable() {
		mouseClick.action.performed -= mouseTargetClicked;
	}

	// https://www.youtube.com/watch?v=HfqRKy5oFDQ
	private void mouseTargetClicked(InputAction.CallbackContext obj) { 
		Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, rayLength, layerMask:~(layersToIgnore))) {
			if (hit.collider != null && (hit.collider.gameObject.TryGetComponent<MouseClickable>(out MouseClickable mouseClickable))) {
				mouseClickable.OnMouseClicked();
			}
		}
	}
}
