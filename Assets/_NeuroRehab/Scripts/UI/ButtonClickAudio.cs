using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class that can be used to add Click Audio to elements when interacting with them.
/// </summary>
public class ButtonClickAudio : MonoBehaviour, IPointerClickHandler {

	private AudioSourceManager audioSourceManager;
	private float buttonAudioRate = 0.25f;
	private float audioPlayed = 0f;

	void Start() {
		audioSourceManager = ObjectManager.Instance.getFirstObjectByName("AudioSourceManager").GetComponent<AudioSourceManager>();
	}

	public void OnPointerClick(PointerEventData eventData) {
		playSound();
	}

	/// <summary>
	/// We use reference to audio source object.
	/// </summary>
	void playSound() {
		if (Time.time < audioPlayed + buttonAudioRate) {
			return;
		}
		audioSourceManager.buttonClickAudio.audioSource.Play();
		audioPlayed = Time.time;
	}
}
