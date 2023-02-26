using UnityEngine;
using UnityEngine.InputSystem;

public class StaticMenuManager : MonoBehaviour
{
	[SerializeField] private InputActionReference menuAction;

	[SerializeField] private MiniMenuVisibilityManager miniMenuVisibilityManager;

	[SerializeField] private Transform cameraTransform;

	private FreezeObjectPosition freezeObjectPosition;
	private FreezeObjectRotation freezeObjectRotation;
	private MiniMenuManager miniMenuManager;

	private bool isMenuShowing = false;

	private void OnEnable() {
		menuAction.action.performed += triggerMenu;

		initElements();
		resetMenu();
	}

	private void OnDisable() {
		menuAction.action.performed -= triggerMenu;
	}

	private void triggerMenu(InputAction.CallbackContext obj) {
		if (miniMenuVisibilityManager != null) {
			if (miniMenuManager == null) {
				miniMenuManager = gameObject.GetComponent<MiniMenuManager>();
			}
			if (miniMenuVisibilityManager.isMenuShowing(miniMenuManager)) {
				return;
			}
			isMenuShowing = miniMenuVisibilityManager.getMenuStatus(miniMenuManager);
		} else {
			isMenuShowing = !isMenuShowing;
		}

		transform.localRotation = cameraTransform.localRotation;
		transform.position = cameraTransform.position + (cameraTransform.forward * 1f);

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
