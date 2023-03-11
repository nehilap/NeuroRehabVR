using UnityEngine;
using UnityEngine.InputSystem;

public class StaticMenuManager : MonoBehaviour
{
	[SerializeField] private InputActionReference menuAction;


	[SerializeField] private Transform cameraTransform;

	[SerializeField] private float menuOffset = 1.25f;

	private FreezeObjectPosition freezeObjectPosition;
	private FreezeObjectRotation freezeObjectRotation;
	private MiniMenuManager miniMenuManager;

	[SerializeField] private bool isMenuShowing = false;

	private void OnEnable() {
		menuAction.action.Enable();

		menuAction.action.performed += triggerMenu;

		initElements();
		resetMenu();
	}

	private void OnDisable() {
		menuAction.action.Disable();

		menuAction.action.performed -= triggerMenu;
	}

	private void triggerMenu(InputAction.CallbackContext obj) {
		isMenuShowing = !isMenuShowing;

		transform.localRotation = cameraTransform.localRotation;
		transform.position = cameraTransform.position + (cameraTransform.forward * menuOffset);

		// first we freeze position + rotation, so that it gets 'saved'
		freezeObjectPosition.enabled = isMenuShowing;
		freezeObjectRotation.enabled = isMenuShowing;
	}

	private void resetMenu() {
		freezeObjectPosition.enabled = false;
		freezeObjectRotation.enabled = false;
	}

	private void initElements() {
		if (freezeObjectPosition == null) {
			freezeObjectPosition = gameObject.GetComponent<FreezeObjectPosition>();
		}
		if (freezeObjectRotation == null) {
			freezeObjectRotation = gameObject.GetComponent<FreezeObjectRotation>();
		}
	}
}
