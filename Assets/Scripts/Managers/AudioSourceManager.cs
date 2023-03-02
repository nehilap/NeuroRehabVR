using UnityEngine;

[System.Serializable]
public class CustomAudioSource {
	public AudioSource audioSource;
	public float initialVolume;
}

public class AudioSourceManager : MonoBehaviour {
	public CustomAudioSource buttonClickAudio;

	private void Start() {
		buttonClickAudio.audioSource.volume = buttonClickAudio.initialVolume * SettingsManager.Instance.audioSettings.UIvolume;
	}
}
