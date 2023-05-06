using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class used to manage mouse triggers and mouse visibility. Used when there are multiple events that cause mouse to become visible (such as Grab + Menu)
/// </summary>
public class MouseManager : MonoBehaviour {

	[SerializeField] private InputActionReference[] mouseVisibilityTriggers;
	[SerializeField] public int activeTriggers = 0;

	[SerializeField] private ShortcutMarkerManager shortcutMarkerManager;

	private void Awake() {
		activeTriggers = 0;
	}

	private void OnEnable() {
		foreach (InputActionReference item in mouseVisibilityTriggers) {
			item.action.performed += triggerVisibility;
		}
	}

	private void OnDisable() {
		foreach (InputActionReference item in mouseVisibilityTriggers) {
			item.action.performed -= triggerVisibility;
		}
	}

	//
	private void triggerVisibility(InputAction.CallbackContext obj) {
		for (int i = 0; i < mouseVisibilityTriggers.Length; i++) {
			if (mouseVisibilityTriggers[i].action == obj.action) {
				activeTriggers = activeTriggers ^ (int) Mathf.Pow(2, i);

				if (shortcutMarkerManager) {
					shortcutMarkerManager.triggerMarker(obj.action.name);
				}
				break;
			}
		}

		if (activeTriggers > 0) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		} else {
			Cursor.lockState = CursorLockMode.Locked;
		}
	}
}
