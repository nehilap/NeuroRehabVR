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
	protected Vector3 initialTransformOffset;

	protected float offsetLimit = 0.7f;

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

	protected Vector3 originalMenuPosition;
	protected Vector3 originalMenuScale;
	protected Transform originalMenuParent;

	public bool isMenuShowing = false;
	protected bool menuInitialized = false;

	protected virtual void Awake() {
		if (miniMenuVisibilityManager != null) {
			miniMenuVisibilityManager.registerMiniMenuManager(this);
		}
		initialTransformOffset = transform.localPosition;
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


		if (offsetControls) {
			initOffset();
		}

		menuInitialized = true;
	}

	protected virtual void initOffset() {
		if (SettingsManager.Instance.offsetSettings.miniMenusOffsetSettingsInitialized) {
			transformOffset = SettingsManager.Instance.offsetSettings.miniMenuTransformOffset;

			transform.localPosition = transformOffset;
		} else {
			transformOffset = transform.localPosition;
			saveOffsetSettings();
			SettingsManager.Instance.offsetSettings.miniMenusOffsetSettingsInitialized = true;
		}
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
				yield break;
			}
			isMenuShowing = miniMenuVisibilityManager.triggerMenu(this);
		} else {
			isMenuShowing = !isMenuShowing;
		}

		if (isMenuShowing) {
			if (isStickyParent) {
				menuToShow.transform.SetParent(menuHolder);
			}

			setupMenuPositining();

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

	protected virtual void setupMenuPositining() {
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
		if (Mathf.Abs(initialTransformOffset.x - transformOffset.x) >= offsetLimit) {
			return;
		}

		transformOffset += new Vector3(0.05f, 0f, 0f);
		transform.localPosition = new Vector3(transform.localPosition.x + 0.05f, transform.localPosition.y, transform.localPosition.z);

		saveOffsetSettings();
	}

	public virtual void offsetLeft() {
		if (Mathf.Abs(initialTransformOffset.x - transformOffset.x) >= offsetLimit) {
			return;
		}
		transformOffset += new Vector3(-0.05f, 0f, 0f);
		transform.localPosition = new Vector3(transform.localPosition.x - 0.05f, transform.localPosition.y, transform.localPosition.z);

		saveOffsetSettings();
	}

	public virtual void offsetFwd() {
		if (Mathf.Abs(initialTransformOffset.z - transformOffset.z) >= offsetLimit) {
			return;
		}
		transformOffset += new Vector3(0f, 0f, 0.05f);
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + 0.05f);

		saveOffsetSettings();
	}

	public virtual void offsetBack() {
		if (Mathf.Abs(initialTransformOffset.z - transformOffset.z) >= offsetLimit) {
			return;
		}

		transformOffset += new Vector3(0f, 0f, -0.05f);
		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 0.05f);

		saveOffsetSettings();
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
		transformOffset = initialTransformOffset;
		transform.localPosition = transformOffset;
		setupMenuPositining();

		saveOffsetSettings();
	}

	protected virtual void saveOffsetSettings() {
		SettingsManager.Instance.offsetSettings.miniMenuTransformOffset = transformOffset;
	}
}
