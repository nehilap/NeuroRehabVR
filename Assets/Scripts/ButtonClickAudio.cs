using UnityEngine;
using UnityEngine.UI;

public class ButtonClickAudio : MonoBehaviour {
	[SerializeField] private AudioSource audioSource;

	void Start() {
		if (audioSource == null) {
			audioSource = AudioSourceManager.Instance.GetComponent<AudioSource>();
		}

		gameObject.GetComponent<Button>().onClick.AddListener(playSound);
	}

	void playSound() {
		audioSource.Play();
	}
}
