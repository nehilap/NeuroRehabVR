using UnityEngine;
using UnityEngine.InputSystem;

public class WristMenuManager : MonoBehaviour {
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Vector3 scale;

    private Canvas _wristUICanvas;
    private InputAction _menu;

    private GameObject therapistMenu;
    private Vector3 originalTherapistMenuPosition;
    private Vector3 originalTherapistMenuScale;
    private Transform originalTherapistMenuParent;

    private void Start() {
        _wristUICanvas = GetComponent<Canvas>();
        _menu = inputActions.FindActionMap("XRI LeftHand").FindAction("Menu");
        _menu.Enable();
        _menu.performed += ToggleMenu;

        therapistMenu = GameObject.Find("TherapistMenu");
        originalTherapistMenuPosition = therapistMenu.transform.localPosition;
        originalTherapistMenuScale = therapistMenu.transform.localScale;
        originalTherapistMenuParent = therapistMenu.transform.parent;
    }

    private void OnDestroy() {
        _menu.performed -= ToggleMenu;
    }

    public void ToggleMenu(InputAction.CallbackContext context) {
        if (!_wristUICanvas.enabled) {
            therapistMenu.transform.SetParent(transform);

            therapistMenu.transform.localScale = scale;
            therapistMenu.transform.localRotation = Quaternion.identity;
            therapistMenu.transform.localPosition = positionOffset;
        } else {
            therapistMenu.transform.SetParent(originalTherapistMenuParent);

            therapistMenu.transform.localScale = originalTherapistMenuScale;
            therapistMenu.transform.localRotation = Quaternion.identity;
            therapistMenu.transform.localPosition = originalTherapistMenuPosition;
        }

        _wristUICanvas.enabled = !_wristUICanvas.enabled;
    }
}
