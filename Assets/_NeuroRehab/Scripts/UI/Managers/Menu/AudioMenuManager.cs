using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class AudioMenuManager : MonoBehaviour {
	[SerializeField] private TMP_Text audioVolumeTextValue;
	[SerializeField] private Slider audioVolumeSlider;

	[Tooltip("2 = 200% of initial volume")][Range(0f, 10f)][SerializeField] private float maxAudioVolume = 2f;

	private void Start() {
		audioVolumeSlider.value = SettingsManager.Instance.audioSettings.UIvolume * audioVolumeSlider.maxValue / maxAudioVolume;
		audioVolumeTextValue.text = $"{Mathf.Round(audioVolumeSlider.value * maxAudioVolume * 100 / audioVolumeSlider.maxValue)} %";
	}

	public void audioVolumeSliderHandler(float value) {
		audioVolumeTextValue.text = $"{Mathf.Round(value * maxAudioVolume * 100 / audioVolumeSlider.maxValue)} %";

		SettingsManager.Instance.audioSettings.UIvolume = (value * maxAudioVolume) / audioVolumeSlider.maxValue;

		CustomAudioSource buttonAudio = ObjectManager.Instance.getFirstObjectByName("AudioSourceManager").GetComponent<AudioSourceManager>().buttonClickAudio;
		buttonAudio.audioSource.volume = buttonAudio.initialVolume * SettingsManager.Instance.audioSettings.UIvolume;
	}
}
