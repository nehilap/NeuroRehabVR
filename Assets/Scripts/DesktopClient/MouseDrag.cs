using UnityEngine;
using UnityEngine.InputSystem;

public class MouseDrag : MonoBehaviour {
    
    [SerializeField] private InputActionReference menuAction;

    [SerializeField] private bool isGrabbing = false;

    private void OnEnable() {
        menuAction.action.performed += triggerGrab;
    }

    private void OnDisable() {
        menuAction.action.performed -= triggerGrab;
    }

    private void triggerGrab(InputAction.CallbackContext obj) { 
        isGrabbing = !isGrabbing;

        if (isGrabbing) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
