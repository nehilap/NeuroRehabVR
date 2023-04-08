using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickAudio : MonoBehaviour {

	[SerializeField] private float buttonAudioRate = 0.3f;
	private AudioSourceManager audioSourceManager;

	private float audioPlayed = 0f;

	void Start() {
		audioSourceManager = ObjectManager.Instance.getFirstObjectByName("AudioSourceManager").GetComponent<AudioSourceManager>();
	}

	private void OnEnable() {
		if (gameObject.TryGetComponent<Button>(out Button button)) {
			button.onClick.AddListener(playSound);
		} else if (gameObject.TryGetComponent<Slider>(out Slider slider)) {
			slider.onValueChanged.AddListener(playSound);
		} else if (gameObject.TryGetComponent<Toggle>(out Toggle toggle)) {
			toggle.onValueChanged.AddListener(playSound);
		} else if (gameObject.TryGetComponent<TMP_Dropdown>(out TMP_Dropdown dropdown)) {
			dropdown.onValueChanged.AddListener(playSound);
		}
	}

	private void OnDisable() {
		if (gameObject.TryGetComponent<Button>(out Button button)) {
			button.onClick.RemoveListener(playSound);
		} else if (gameObject.TryGetComponent<Slider>(out Slider slider)) {
			slider.onValueChanged.RemoveListener(playSound);
		} else if (gameObject.TryGetComponent<Toggle>(out Toggle toggle)) {
			toggle.onValueChanged.RemoveListener(playSound);
		} else if (gameObject.TryGetComponent<TMP_Dropdown>(out TMP_Dropdown dropdown)) {
			dropdown.onValueChanged.RemoveListener(playSound);
		}
	}

	// we reference audio source object,
	// we have to check null, in case it's called even before Start method played out
	// (due to nature of multiplayer, this could happen on sliders for example)
	void playSound() {
		if (ObjectManager.Instance.getFirstObjectByName("AudioSourceManager") == null) {
			return;
		}
		if (Time.time < audioPlayed + buttonAudioRate) {
			return;
		}
		audioSourceManager.buttonClickAudio.audioSource.Play();
		audioPlayed = Time.time;
	}

	// Wrapper for method
	void playSound(int val) {
		playSound();
	}

	// Wrapper for method
	void playSound(float val) {
		playSound();
	}

	// Wrapper for method
	void playSound(bool val) {
		playSound();
	}
}
