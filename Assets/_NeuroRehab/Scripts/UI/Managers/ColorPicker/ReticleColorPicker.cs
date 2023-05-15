using UnityEngine;

public class ReticleColorPicker : MonoBehaviour {
	[SerializeField] private ColorPickerManager colorPickerManager;

	void Awake() {
		colorPickerManager.ChosenColor = SettingsManager.Instance.generalSettings.ReticleColor;

		colorPickerManager.OnColorChange += changeReticleColorSettings;
	}

	private void changeReticleColorSettings() {
		SettingsManager.Instance.generalSettings.ReticleColor = colorPickerManager.ChosenColor;
	}
}
