using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MiniMenuManager : MonoBehaviour {

	[SerializeField] private InputActionReference menuAction;
	[SerializeField] private bool lockCursor;
	[SerializeField] private bool isStickyParent = true;
	[SerializeField] private Transform menuHolder;
	[SerializeField] private GameObject menuToShow;
	[SerializeField] private string menuNameToShow;

	protected Vector3 transformOffset;
	protected Vector3 initTransformOffset;

	[SerializeField] private Vector3 positionOffset;
	[SerializeField] private Vector3 scale;

	[SerializeField] private bool offsetByWidth;
	[SerializeField] private bool offsetByHeight;

	[SerializeField] private MiniMenuVisibilityManager miniMenuVisibilityManager;

	[SerializeField] private GameObject reticle;

	[SerializeField] private GameObject offsetControls;
	[SerializeField] private Button offsetControlsButton;
	[SerializeField] private Sprite activeOffsetControlsButton;
	[SerializeField] private Sprite inactiveOffsetControlsButton;

	private Vector3 originalMenuPosition;
	private Vector3 originalMenuScale;
	private Transform originalMenuParent;

	public bool isMenuShowing = false;
	private bool menuInitialized = false;

	protected virtual void Awake() {
		if (miniMenuVisibilityManager != null) {
			miniMenuVisibilityManager.registerMiniMenuManager(this);
		}
	}

	protected virtual void Start() {
		menuHolder.GetComponent<Canvas>().enabled = false;
		if (offsetControls) {
			offsetControls.SetActive(false);
		}

		initMenu();
	}

	private void initMenu() {
		if (menuInitialized) {
			return;
		}

		if (menuToShow == null && menuNameToShow.Trim().Length > 0) {
			menuToShow = ObjectManager.Instance.getFirstObjectByName(menuNameToShow);
		}
		originalMenuPosition = menuToShow.transform.localPosition;
		originalMenuScale = menuToShow.transform.localScale;
		originalMenuParent = menuToShow.transform.parent;

		transformOffset = transform.localPosition;
		initTransformOffset = transformOffset;

		menuInitialized = true;
	}

	private void OnEnable() {
		menuAction.action.Enable();

		menuAction.action.performed += triggerMenu;

		if (originalMenuParent != null) {
			resetMenu();
		}
	}

	private void OnDisable() {
		menuAction.action.Disable();

		menuAction.action.performed -= triggerMenu;

		initMenu();

		if (reticle) {
			reticle.SetActive(true);
		}
	}

	private void triggerMenu(InputAction.CallbackContext obj) {
		StartCoroutine(triggerMenuCoroutine());
	}

	/// <summary>
	/// Coroutine for triggering menu, we have to wrap it in Coroutine and wait for Fixed update manually, because Hold action acts weirdly, triggers unwanted artifacts in VR.
	/// </summary>
	/// <returns></returns>
	private IEnumerator triggerMenuCoroutine() {
		yield return new WaitForFixedUpdate();

		if (miniMenuVisibilityManager != null) {
			if (miniMenuVisibilityManager.isMenuShowing(this)) {
				yield return null;
			}
			isMenuShowing = miniMenuVisibilityManager.triggerMenu(this);
		} else {
			isMenuShowing = !isMenuShowing;
		}

		if (isMenuShowing) {
			if (isStickyParent) {
				menuToShow.transform.SetParent(menuHolder);
			}

			renderMenu();

			if (reticle) {
				reticle.SetActive(false);
			}
		} else {
			menuToShow.transform.SetParent(originalMenuParent);

			menuToShow.transform.localScale = originalMenuScale;
			menuToShow.transform.localRotation = Quaternion.identity;
			menuToShow.transform.localPosition = originalMenuPosition;

			if (reticle) {
				reticle.SetActive(true);
			}
		}

		menuHolder.GetComponent<Canvas>().enabled = isMenuShowing;
	}

	protected virtual void renderMenu() {
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
	}

	private void resetMenu() {
		if (miniMenuVisibilityManager != null) {
			miniMenuVisibilityManager.setMenuStatus(this, false);
			isMenuShowing = false;
		} else {
			isMenuShowing = false;
		}

		menuHolder.GetComponent<Canvas>().enabled = isMenuShowing;

		menuToShow.transform.SetParent(originalMenuParent);

		menuToShow.transform.localScale = originalMenuScale;
		menuToShow.transform.localRotation = Quaternion.identity;
		menuToShow.transform.localPosition = originalMenuPosition;
	}

	public virtual void offsetRight() {
		transformOffset += new Vector3(0.05f, 0f, 0f);
		transform.localPosition = new Vector3(transform.localPosition.x + 0.05f, transform.localPosition.y, transform.localPosition.z);
	}

	public virtual void offsetLeft() {
		transformOffset += new Vector3(-0.05f, 0f, 0f);
		transform.localPosition = new Vector3(transform.localPosition.x - 0.05f, transform.localPosition.y, transform.localPosition.z);
	}

	public virtual void offsetFwd() {
		transformOffset += new Vector3(0f, 0f, 0.05f);
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + 0.05f);
	}

	public virtual void offsetBack() {
		transformOffset += new Vector3(0f, 0f, -0.05f);
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 0.05f);
	}

	public void triggerOffset() {
		offsetControls.SetActive(!offsetControls.activeSelf);

		if (offsetControls.activeSelf) {
			offsetControlsButton.image.sprite = activeOffsetControlsButton;
		} else {
			offsetControlsButton.image.sprite = inactiveOffsetControlsButton;
		}
	}

	public void resetOffset() {
		transformOffset = initTransformOffset;
		transform.localPosition = transformOffset;
		renderMenu();
	}
}
