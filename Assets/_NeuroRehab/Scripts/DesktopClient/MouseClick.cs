using UnityEngine;
using UnityEngine.InputSystem;

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

	/// <summary>
	/// Triggers MouseCLick event on object that has correct component.
	/// </summary>
	/// <param name="obj"></param>
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
