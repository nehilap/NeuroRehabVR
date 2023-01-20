using UnityEngine;
using UnityEngine.UI;

public class ButtonClickAudio : MonoBehaviour
{
    public static AudioSource audioSource;
    void Start() {
        if (audioSource == null) {
            audioSource = GameObject.Find("ButtonAudioSource")?.GetComponent<AudioSource>();
        }

        gameObject.GetComponent<Button>().onClick.AddListener(playSound);
    }

    void playSound() {
        audioSource.Play();
    }
}
