using UnityEngine;
using UnityEngine.UI;

public class ButtonClickAudio : MonoBehaviour {
	void Start() {

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
		if (ObjectManager.Instance.getFirstObjectByName("AudioSourceManager") == null) {
			return;
		}
		ObjectManager.Instance.getFirstObjectByName("AudioSourceManager").GetComponent<AudioSourceManager>().buttonClickAudio.audioSource.Play();
	}

	// Wrapper for method
	void playSoundSlider(float val) {
		playSound();
	}
}
