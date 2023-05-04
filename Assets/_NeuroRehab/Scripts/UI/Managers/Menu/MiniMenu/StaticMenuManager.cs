using UnityEngine;

public class StaticMenuManager : MiniMenuManager {

	[SerializeField] private Transform cameraTransform;

	[SerializeField] private float menuOffset = 1.25f;
	[SerializeField] private float menuOffsetVertical = -0.25f;

	protected override void Awake() {
		base.Awake();
		transform.position = cameraTransform.position + (cameraTransform.forward * menuOffset);
		transform.localRotation = cameraTransform.localRotation;
	}

	protected override void initOffset() {
		if (SettingsManager.Instance.offsetSettings.staticMenusOffsetSettingsInitialized) {
			transformOffset = SettingsManager.Instance.offsetSettings.staticMenuTransformOffset;

			transform.localPosition = transformOffset;
		} else {
			transformOffset = transform.localPosition;
			saveOffsetSettings();
			SettingsManager.Instance.offsetSettings.staticMenusOffsetSettingsInitialized = true;
		}
	}

	protected override void setupMenuPositining() {
		base.setupMenuPositining();
		if (isMenuShowing) {
			transform.position = cameraTransform.position + (cameraTransform.forward * transformOffset.z);
			transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
			transform.SetParent(null);

			transform.position += new Vector3(0f, menuOffsetVertical, 0f);
		}
	}

	public override void offsetRight() {
		if (Mathf.Abs(initialTransformOffset.x - transformOffset.x) >= offsetLimit) {
			return;
		}

		transformOffset += new Vector3(0.05f, 0f, 0f);
		transform.localPosition += transform.right * 0.05f;

		saveOffsetSettings();
	}

	public override void offsetLeft() {
		if (Mathf.Abs(initialTransformOffset.x - transformOffset.x) >= offsetLimit) {
			return;
		}

		transformOffset += new Vector3(-0.05f, 0f, 0f);
		transform.localPosition += transform.right * -0.05f;

		saveOffsetSettings();
	}

	public override void offsetFwd() {
		if (Mathf.Abs(initialTransformOffset.z - transformOffset.z) >= offsetLimit) {
			return;
		}

		transformOffset += new Vector3(0f, 0f, 0.05f);
		transform.localPosition += transform.forward * 0.05f;

		saveOffsetSettings();
	}

	public override void offsetBack() {
		if (Mathf.Abs(initialTransformOffset.z - transformOffset.z) >= offsetLimit) {
			return;
		}

		transformOffset += new Vector3(0f, 0f, -0.05f);
		transform.localPosition += transform.forward * -0.05f;

		saveOffsetSettings();
	}

	protected override void saveOffsetSettings() {
		SettingsManager.Instance.offsetSettings.staticMenuTransformOffset = transformOffset;
	}
}
