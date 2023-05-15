using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace NeuroRehab.Settings {
	[Serializable]
	public class AudioSettings {
		[SerializeField] private AudioMixer mixer;
		[SerializeField] private float _UIvolume = 1f;

		public float UIvolume { get => _UIvolume; set {
				_UIvolume = value;
				if (mixer)
					mixer.SetFloat("UIVol", Mathf.Log10(value) * 20);
			}
		}

		public AudioSettings(AudioMixer _mixer) {
			mixer = _mixer;
		}

		public IEnumerator delayedSetAudioSettingsUpdate(AudioSettingsSerialized audioSettings) {
			_UIvolume = audioSettings.UIvolume;

			yield return new WaitUntil(() => mixer);
			mixer.SetFloat("UIVol", Mathf.Log10(UIvolume) * 20);
		}
	}

	/// <summary>
	/// Custom class made for Serialization, so that our Settings classes can use objects from scene etc...
	/// </summary>
	[Serializable]
	public class AudioSettingsSerialized {
		[SerializeField] public float UIvolume = 1f;

		public AudioSettingsSerialized(AudioSettings audioSettings) {
			UIvolume = audioSettings.UIvolume;
		}
	}
}