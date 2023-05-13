using System.Collections.Generic;
using UnityEngine;
using NeuroRehab.Settings;
using UnityEngine.Audio;

/// <summary>
/// Contains various settings - RoleSettings, AvatarSettings, AudioSettings, GeneralSettings
/// </summary>
public class SettingsManager : MonoBehaviour, ISaveable {
	private static SettingsManager _instance;
	public static SettingsManager Instance {
		get {
			return _instance;
		}
	}
	[SerializeField] public Sprite reticleSpriteFilled;
	[SerializeField] public Sprite reticleSpriteEmpty;

	[SerializeField] public bool isLogEnabled = true;
	[SerializeField] private AudioMixer mixer;

	public bool initializedFromFile = false;
	public string saveFile = "SaveConfig.dat";

	[Header("Avatars prefabs used")]
	[SerializeField] public List<GameObject> avatarMalePrefabs = new List<GameObject>();
	[SerializeField] public List<GameObject> avatarFemalePrefabs = new List<GameObject>();

	[Header("Persistent Settings")]
	[SerializeField] public AvatarSettings avatarSettings = new AvatarSettings();
	[SerializeField] public NeuroRehab.Settings.AudioSettings audioSettings;
	[SerializeField] public GraphicsSettings graphicsSettings = new GraphicsSettings();
	[SerializeField] public GeneralSettings generalSettings = new GeneralSettings();
	[SerializeField] public RoleSettings roleSettings = new RoleSettings();
	[SerializeField] public OffsetSettings offsetSettings = new OffsetSettings();

	[SerializeField] public string ipAddress;

	/// <summary>
	/// Settings version string is used to invalidate settings file when reading it, it can be virtually anything.
	/// Can be used to reset settings to default values. When pushing new version to git with changes to structure it's good to change version, so that other devs will also update settings file when running the game.
	/// </summary>
	private string settingsVersion = "1b";

	void Awake() {
		{
			if (_instance != null && _instance != this )
			{
				Destroy(gameObject);
				return;
			}
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}

		generalSettings.fpsCounterFilePath = Application.persistentDataPath + "/fps.txt";
		ipAddress = "localhost";
		graphicsSettings.SetRenderScale(graphicsSettings.renderScale);

		// fast solution, would be better to rework it to use custom logger, so that we can control better what is / isn't logged
		// https://gamedevbeginner.com/how-to-use-debug-log-in-unity-without-affecting-performance/
		Debug.unityLogger.logEnabled = isLogEnabled;

		audioSettings = new NeuroRehab.Settings.AudioSettings(mixer);

		try {
			loadSettings();
		} catch (System.Exception e) {
			Debug.Log(e);
		}
	}

	void Start() {
		Application.targetFrameRate = 60;
	}

	void OnApplicationQuit() {
		saveSettings();
	}

	private void saveSettings() {
		SaveData sd = new SaveData();
		Instance.PopulateSaveData(sd);

		if (FileManager.WriteToFile(saveFile, sd.ToJson())) {
			Debug.Log("Succesfully saved Settings");
		}
	}

	private void loadSettings() {
		if (FileManager.LoadFromFile(saveFile, out string json)) {
			SaveData sd = new SaveData();
			sd.LoadFromJson(json);

			if (!sd.SettingsVersion.Equals(settingsVersion)) {
				Debug.Log("Invalid Settings Version, settings will not be applied!!");
			} else {
				Instance.LoadFromSaveData(sd);
				Debug.Log("Settings sucesfully initialized");
				initializedFromFile = true;
			}
			graphicsSettings.SetRenderScale(graphicsSettings.renderScale);
		}
	}

	public void PopulateSaveData(SaveData saveData) {
		saveData.AudioSettings = new AudioSettingsSerialized(audioSettings);
		saveData.AvatarSettings = avatarSettings;
		saveData.RoleSettings = roleSettings;
		saveData.OffsetSettings = offsetSettings;
		saveData.GraphicsSettings = graphicsSettings;
		saveData.GeneralSettings = generalSettings;
		saveData.IpAddress = ipAddress;
		saveData.SettingsVersion = settingsVersion;
	}

	public void LoadFromSaveData(SaveData saveData) {
		audioSettings = new NeuroRehab.Settings.AudioSettings(mixer);
		StartCoroutine(audioSettings.delayedSetAudioSettingsUpdate(saveData.AudioSettings));

		avatarSettings = saveData.AvatarSettings;
		roleSettings = saveData.RoleSettings;
		offsetSettings = saveData.OffsetSettings;
		graphicsSettings = saveData.GraphicsSettings;
		generalSettings = saveData.GeneralSettings;
		ipAddress = saveData.IpAddress;
		settingsVersion = saveData.SettingsVersion;
	}
}
