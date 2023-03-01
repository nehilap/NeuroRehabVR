using UnityEngine;
using UnityEngine.UI;

public class ButtonClickAudio : MonoBehaviour {
	[SerializeField] private AudioSource audioSource;

	void Start() {
		if (audioSource == null) {
			audioSource = AudioSourceManager.Instance.GetComponent<AudioSource>();
		}

		if (gameObject.TryGetComponent<Button>(out Button button)) {
			button.onClick.AddListener(playSound);
		} else if (gameObject.TryGetComponent<Slider>(out Slider slider)) {
			slider.onValueChanged.AddListener(playSoundSlider);
		}
	}

	// we reference audio source object,
	// we have to check null, in case it's called even before Start method played out
	// (due to nature of multiplayer, this could happen on sliders for example)
	void playSound() {
		if (audioSource == null) {
			return;
		}

		audioSource.Play();
	}

	// Wrapper for method
	void playSoundSlider(float val) {
		playSound();
	}
}
