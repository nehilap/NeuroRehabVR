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
	[Tooltip("2 = 200% of initial volume")][Range(0f, 2f)] public float UIvolume = 1f;
}

[System.Serializable]
public class GeneralSettings {
	public bool showFps = true;
	public bool writeFps = false;
	public string fpsCounterFilePath;
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
	[SerializeField] public GeneralSettings generalSettings = new GeneralSettings();

	void Awake() {
		generalSettings.fpsCounterFilePath = Application.persistentDataPath + "/fps.txt";
	}
	void Start() {
		Application.targetFrameRate = 60;
	}
}
