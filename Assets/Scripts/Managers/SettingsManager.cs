using UnityEngine;

[System.Serializable]
public class AvatarSettings {
	public bool isFemale;
	public int avatarNumber;
	public float sizeMultiplier;
	public float offsetDistance;
	public GameObject currentModel;
}

[System.Serializable]
public class AudioSettings {
	[Range(0f, 2f)] public float UIvolume = 1f;
}

public class SettingsManager : MonoBehaviour {

	private static SettingsManager _instance;
	public static SettingsManager Instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<SettingsManager>();
			}
			return _instance;
		}
	}

	[SerializeField] public AvatarSettings avatarSettings = new AvatarSettings();
	[SerializeField] public AudioSettings audioSettings = new AudioSettings();

	void Start() {
		Application.targetFrameRate = 60;
	}
}
