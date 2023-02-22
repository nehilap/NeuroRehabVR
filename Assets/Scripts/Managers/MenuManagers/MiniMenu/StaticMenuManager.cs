using UnityEngine;
using UnityEngine.InputSystem;

public class StaticMenuManager : MonoBehaviour
{
	[SerializeField] private InputActionReference menuAction;

	[SerializeField] private MiniMenuVisibilityManager miniMenuVisibilityManager;

	private DirectionalOffset directionalOffset;
	private FollowObject followObject;
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

		// first we freeze position + rotation, so that it gets 'saved'
		freezeObjectPosition.enabled = isMenuShowing;
		freezeObjectRotation.enabled = isMenuShowing;

		// now we can disable following object + offset
		followObject.enabled = !isMenuShowing;
		directionalOffset.enabled = !isMenuShowing;
	}

	private void resetMenu() {
		freezeObjectPosition.enabled = false;
		freezeObjectRotation.enabled = false;

		followObject.enabled = true;
		directionalOffset.enabled = true;
	}

	private void initElements() {
		if (directionalOffset == null) {
			directionalOffset = gameObject.GetComponent<DirectionalOffset>();
		}
		if (followObject == null) {
			followObject = gameObject.GetComponent<FollowObject>();
		}
		if (freezeObjectPosition == null) {
			freezeObjectPosition = gameObject.GetComponent<FreezeObjectPosition>();
		}
		if (freezeObjectRotation == null) {
			freezeObjectRotation = gameObject.GetComponent<FreezeObjectRotation>();
		}
	}
}
