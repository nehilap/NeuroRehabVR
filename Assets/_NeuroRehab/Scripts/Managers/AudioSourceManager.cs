using UnityEngine;

[System.Serializable]
public class CustomAudioSource {
	public AudioSource audioSource;
}

/// <summary>
/// Audio source manager for audio such as Button clicks etc. We can use this when we want to play Mono Audio.
/// </summary>
public class AudioSourceManager : MonoBehaviour {
	public CustomAudioSource buttonClickAudio;
}
