using System.Collections.Generic;
using Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class SettingsMenuManager : MonoBehaviour {

	[SerializeField] private Toggle measureFpsToggle;
	[SerializeField] private Toggle writeFpsToggle;

	[SerializeField] private TMP_Text renderScaleTextValue;
	[SerializeField] private Slider renderScaleSlider;

	[SerializeField] private TMP_Text reticleScaleTextValue;
	[SerializeField] private Slider reticleScaleSlider;

	[SerializeField] private TMP_Dropdown reticleStyleDropdown;

	private void Start() {
		measureFpsToggle.isOn = SettingsManager.Instance.generalSettings.measureFps;
		writeFpsToggle.isOn = SettingsManager.Instance.generalSettings.writeFps;

		renderScaleSlider.value = SettingsManager.Instance.currentRenderScale * 10;
		renderScaleTextValue.text = $"{SettingsManager.Instance.currentRenderScale}";

		reticleScaleSlider.value = SettingsManager.Instance.generalSettings.ReticleScale * 10;
		reticleScaleTextValue.text = $"{SettingsManager.Instance.generalSettings.ReticleScale * 100} %";

		reticleStyleDropdown.value = reticleStyleDropdown.options.FindIndex(option => option.text.ToLower().Equals(SettingsManager.Instance.generalSettings.ReticleStyle.ToString().ToLower()));
		reticleStyleDropdown.onValueChanged.AddListener(delegate {
			reticleStyleHandler(reticleStyleDropdown);
		});
	}

	public void setControllerPrefabs(int controller) {
		if (!System.Enum.IsDefined(typeof(ControllerType), controller)) {
			Debug.LogError("Wrong enum value argument - ControllerType");
			return;
		}

		GameObject controllerL = null;
		GameObject controllerR = null;

		foreach (GameObject item in XRSettingsManager.Instance.controllerPrefabs) {
			if (item.name.Contains(((ControllerType) controller).ToString())) {
				if (item.name.Contains("Left")) {
					controllerL = item;
				}else if (item.name.Contains("Right")) {
					controllerR = item;
				}
			}
		}

		XRSettingsManager.Instance.controllerType = (ControllerType) controller;

		XRBaseController rightC = GameObject.Find("RightHand Controller").GetComponent<XRBaseController>();
		XRBaseController leftC = GameObject.Find("LeftHand Controller").GetComponent<XRBaseController>();

		leftC.modelPrefab = controllerL.transform;
		rightC.modelPrefab = controllerR.transform;

		if (rightC.model != null && leftC.model != null) {
			rightC.model.gameObject.SetActive(false);
			leftC.model.gameObject.SetActive(false);
		}

		if (leftC.modelParent != null) {
			leftC.model = Instantiate(leftC.modelPrefab, leftC.modelParent.transform.position, leftC.modelParent.transform.rotation, leftC.modelParent.transform);
		}
		if (rightC.modelParent != null) {
			rightC.model = Instantiate(rightC.modelPrefab, rightC.modelParent.transform.position, rightC.modelParent.transform.rotation, rightC.modelParent.transform);
		}
	}

	public void startXR() {
		StartCoroutine(XRSettingsManager.Instance.startXR());
	}

	public void stopXR() {
		XRSettingsManager.Instance.stopXR();
	}

	public void toggleFps(bool value) {
		SettingsManager.Instance.generalSettings.measureFps = value;

		List<GameObject> counters = ObjectManager.Instance.getObjectsByName("FPSCounter");
		foreach (var counter in counters) {
			counter.GetComponent<FPSCounterManager>().enabled = value;
		}
	}

	public void toggleFpsWrite(bool value) {
		SettingsManager.Instance.generalSettings.writeFps = value;

		if (value) {
			SettingsManager.Instance.generalSettings.measureFps = true;
			measureFpsToggle.isOn = true;
		}
	}

	public void renderScaleSliderHandler(float value) {
		SettingsManager.Instance.SetRenderScale(value / 10);
		renderScaleTextValue.text = $"{value / 10}";
	}

	public void reticleScaleSliderHandler(float value) {
		SettingsManager.Instance.generalSettings.ReticleScale = (value / 10);
		reticleScaleTextValue.text = $"{value * 10} %";

		foreach (var reticle in ObjectManager.Instance.getObjectsByName("Reticle")) {
			if (reticle.TryGetComponent<DesktopReticleManager>(out DesktopReticleManager desktopReticleManager)) {
				desktopReticleManager.updateReticleScale();
			}
		}
	}

	public void reticleStyleHandler(TMP_Dropdown dropdown) {
		switch (dropdown.options[dropdown.value].text) {
			case "Filled": SettingsManager.Instance.generalSettings.ReticleStyle = ReticleStyle.FILLED; break;
			case "Empty": SettingsManager.Instance.generalSettings.ReticleStyle = ReticleStyle.EMPTY; break;
			default: return;
		}

		/*foreach (var reticle in ObjectManager.Instance.getObjectsByName("Reticle")) {
			if (reticle.TryGetComponent<DesktopReticleManager>(out DesktopReticleManager desktopReticleManager)) {
				desktopReticleManager.updateReticleStyle();
			}
			if (reticle.TryGetComponent<CustomXRInteractorLineVisual>(out CustomXRInteractorLineVisual xrReticle)) {
				xrReticle.updateReticleStyle();
			}
		}*/
	}
}
