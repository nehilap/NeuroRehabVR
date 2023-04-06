using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDrop : MonoBehaviour {
	[SerializeField] private InputActionReference mouseClick;
	[SerializeField] private MiniMenuManager miniMenuManager;

	[SerializeField] [Range(0.01f, 100f)] private float mousePhysicsDragSpeed = 10f;
	[SerializeField] [Range(0.01f, 10f)] private float mouseDragSpeed = 0.1f;

	[SerializeField] private Camera mainCamera;

	[SerializeField] private LayerMask layersToIgnore;

	[SerializeField][Range(0.1f, 50f)] private float rayLength = 10f;

	private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

	private Vector3 velocity = Vector3.zero;

	private void Awake() {
		if (mainCamera == null) {
			mainCamera = Camera.current;
		}
	}

	private void OnEnable() {
		mouseClick.action.performed += mousePressed;
	}

	private void OnDisable() {
		mouseClick.action.performed -= mousePressed;
	}

	// https://www.youtube.com/watch?v=HfqRKy5oFDQ
	private void mousePressed(InputAction.CallbackContext obj) {
		if (miniMenuManager) {
			if (miniMenuManager.isMenuShowing) {
				return;
			}
		}

		Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, rayLength, layerMask:~(layersToIgnore))) {
			if (hit.collider != null && (hit.collider.gameObject.CompareTag("Draggable") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Target")
			|| hit.collider.gameObject.GetComponent<DragInterface>() != null)) {
				StartCoroutine(dragUpdate(hit.collider.gameObject));
			}
		}
	}

	private IEnumerator dragUpdate(GameObject draggedObject) {
		Quaternion initRotation = draggedObject.transform.rotation;
		draggedObject.transform.TryGetComponent<DragInterface>(out DragInterface dragInterface);

		dragInterface?.OnStartDrag();
		dragInterface?.OnShowDragRange();

		if (draggedObject.transform.TryGetComponent<NetworkIdentity>(out NetworkIdentity objectIdentity)) {
			if (!objectIdentity.isOwned) {
				NetworkCharacterManager.localNetworkClientInstance.CmdSetItemAuthority(objectIdentity, CharacterManager.localClientInstance.GetComponent<NetworkIdentity>());
			}
		}

		float initDistance = Vector3.Distance(draggedObject.transform.position, mainCamera.transform.position);
		draggedObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody);

		while (mouseClick.action.ReadValue<float>() != 0) {
			Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
			if (rigidbody != null) {
				Vector3 direction = ray.GetPoint(initDistance) - draggedObject.transform.position;
				rigidbody.velocity = direction * mousePhysicsDragSpeed;
				draggedObject.transform.rotation = initRotation;
				yield return waitForFixedUpdate;
			} else {
				draggedObject.transform.position = Vector3.SmoothDamp(draggedObject.transform.position, ray.GetPoint(initDistance), ref velocity, mouseDragSpeed);
				yield return null;
			}
		}

		dragInterface?.OnStopDrag();
		dragInterface?.OnHideDragRange();
	}
}
