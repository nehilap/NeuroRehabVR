using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniMenuManager : MonoBehaviour {
	
	[SerializeField] private InputActionReference menuAction;
	[SerializeField] private bool lockCursor;
	[SerializeField] private Transform menuHolder;
	[SerializeField] private GameObject menuToShow;
	[SerializeField] private string menuNameToShow;
	[SerializeField] private Vector3 positionOffset;
	[SerializeField] private Vector3 scale;

	[SerializeField] private bool offsetByWidth;
	[SerializeField] private bool offsetByHeight;
	
	private Vector3 originalMenuPosition;
	private Vector3 originalMenuScale;
	private Transform originalMenuParent;

	private bool isMenuShowing = false;

	private void Start() {
		menuHolder.GetComponent<Canvas>().enabled = false;

		if (menuToShow == null && menuNameToShow.Trim().Length > 0) {
			menuToShow = ObjectManager.Instance.getMenuByName(menuNameToShow);
		}

		originalMenuPosition = menuToShow.transform.localPosition;
		originalMenuScale = menuToShow.transform.localScale;
		originalMenuParent = menuToShow.transform.parent;
	}

	private void OnEnable() {
		menuAction.action.performed += triggerMenu;

		if (originalMenuParent != null) {
			resetMenu();
		}
	}

	private void OnDisable() {
		menuAction.action.performed -= triggerMenu;
	}

	private void triggerMenu(InputAction.CallbackContext obj) {
		isMenuShowing = !isMenuShowing;

		menuHolder.GetComponent<Canvas>().enabled = isMenuShowing;

		if (isMenuShowing) {
			menuToShow.transform.SetParent(menuHolder);

			menuToShow.transform.localScale = scale;
			menuToShow.transform.localRotation = Quaternion.identity;

			Vector3 newPosition = positionOffset;
			if (offsetByWidth) {
				newPosition.x -= ((RectTransform)menuToShow.transform).rect.width;
			}
			if (offsetByHeight) {
				newPosition.y -= ((RectTransform)menuToShow.transform).rect.height;
			}

			menuToShow.transform.localPosition = newPosition;
		} else {
			menuToShow.transform.SetParent(originalMenuParent);

			menuToShow.transform.localScale = originalMenuScale;
			menuToShow.transform.localRotation = Quaternion.identity;
			menuToShow.transform.localPosition = originalMenuPosition;
		}
	}

	private void resetMenu() {
		isMenuShowing = false;

		menuHolder.GetComponent<Canvas>().enabled = isMenuShowing;

		menuToShow.transform.SetParent(originalMenuParent);

		menuToShow.transform.localScale = originalMenuScale;
		menuToShow.transform.localRotation = Quaternion.identity;
		menuToShow.transform.localPosition = originalMenuPosition;
	}
}
