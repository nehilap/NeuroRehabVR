using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour {
    
    [SerializeField] private InputActionReference mouseX;
    [SerializeField] private InputActionReference mouseY;
    [SerializeField] private InputActionReference menu;

    [SerializeField] private Transform player;

    [SerializeField] private float verticalRotation = 0f;
    [SerializeField] private float mouseSensitivityX = 50f;
    [SerializeField] private float mouseSensitivityY = 50f;

    private bool isMenuShowing = false;

    private void OnEnable() {
        menu.action.performed += triggerMenu;

        isMenuShowing = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable() {
        menu.action.performed -= triggerMenu;
            
        Cursor.lockState = CursorLockMode.None;             
        Cursor.visible = true;
    }

    void LateUpdate() {
        if (isMenuShowing) {
            return;
        }

        float _mouseX =  mouseX.action.ReadValue<float>() * mouseSensitivityX * Time.deltaTime;
        float _mouseY = mouseY.action.ReadValue<float>() * mouseSensitivityY * Time.deltaTime;

        verticalRotation -= _mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        player.Rotate(Vector3.up * _mouseX);
    }
    

    private void triggerMenu(InputAction.CallbackContext obj) {
        isMenuShowing = !isMenuShowing;
    }
}
