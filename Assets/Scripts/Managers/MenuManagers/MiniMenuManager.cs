using UnityEngine;
using UnityEngine.InputSystem;

public class MiniMenuManager : MonoBehaviour {
    
    [SerializeField] private InputActionReference menu;
    [SerializeField] private Transform menuHolder;
    [SerializeField] private GameObject menuToShow;
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

        originalMenuPosition = menuToShow.transform.localPosition;
        originalMenuScale = menuToShow.transform.localScale;
        originalMenuParent = menuToShow.transform.parent;
    }

    private void OnEnable() {
        menu.action.performed += triggerMenu;
    }

    private void OnDisable() {
        menu.action.performed -= triggerMenu;
    }

    private void triggerMenu(InputAction.CallbackContext obj) {
        isMenuShowing = !isMenuShowing;

        menuHolder.GetComponent<Canvas>().enabled = isMenuShowing;

        if (isMenuShowing) {
            Cursor.lockState = CursorLockMode.None;

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
            Cursor.lockState = CursorLockMode.Locked;

            menuToShow.transform.SetParent(originalMenuParent);

            menuToShow.transform.localScale = originalMenuScale;
            menuToShow.transform.localRotation = Quaternion.identity;
            menuToShow.transform.localPosition = originalMenuPosition;
        }
    }
}
