using UnityEngine;

[RequireComponent(typeof(ColorPickerManager))]
public class ReticleColorPicker : MonoBehaviour {
	private ColorPickerManager colorPickerManager;

	void Awake() {
		colorPickerManager = gameObject.GetComponent<ColorPickerManager>();
		colorPickerManager.ChosenColor = SettingsManager.Instance.generalSettings.ReticleColor;

		colorPickerManager.OnColorChange += changeReticleColorSettings;
	}

	private void changeReticleColorSettings() {
		SettingsManager.Instance.generalSettings.ReticleColor = colorPickerManager.ChosenColor;
	}
}
