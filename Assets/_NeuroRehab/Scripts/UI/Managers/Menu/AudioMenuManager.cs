using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class AudioMenuManager : MonoBehaviour {
	[Header("Audio Sliders")]
	[SerializeField] private TMP_Text audioVolumeTextValue;
	[SerializeField] private Slider audioVolumeSlider;

	private void Start() {
		audioVolumeSlider.value = SettingsManager.Instance.audioSettings.UIvolume;
		audioVolumeTextValue.text = $"{Mathf.Round(audioVolumeSlider.value * 100)} %";
	}

	public void UIAudioVolumeSliderHandler(float value) {
		audioVolumeTextValue.text = $"{Mathf.Round(value * 100)} %";

		SettingsManager.Instance.audioSettings.UIvolume = value;
	}
}
