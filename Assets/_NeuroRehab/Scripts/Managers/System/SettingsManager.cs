using System;
using System.Collections.Generic;
using Enums;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class AvatarSettings {
	public bool isFemale;
	public int avatarNumber;
	public float sizeMultiplier;
	public float offsetDistance;
}

[Serializable]
public class RoleSettings {
	public UserRole characterRole;

	public void createCharacter(UserRole role) {
		characterRole = role;
	}
}

[Serializable]
public class AudioSettings {
	[Tooltip("2 = 200% of initial volume")]
	[Range(0f, 2f)] public float UIvolume = 1f;
}

[Serializable]
public class GraphicsSettings {
	public float renderScale;
}

[Serializable]
public class GeneralSettings {
	public bool measureFps = true;
	public bool writeFps = false;
	public string fpsCounterFilePath;

	public float reticleScale = 1f;

	public ReticleStyle reticleStyle = ReticleStyle.EMPTY;
}

[Serializable]
public class OffsetSettings {
	public bool miniMenusOffsetSettingsInitialized = false;
	public bool staticMenusOffsetSettingsInitialized = false;
	public Vector3 miniMenuTransformOffset;
	public Vector3 staticMenuTransformOffset;
}

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

	private UniversalRenderPipelineAsset m_cachedRenderPipeline;
	public UniversalRenderPipelineAsset CachedRenderPipeline{
		get
		{
			if (m_cachedRenderPipeline == null)
				m_cachedRenderPipeline = (UniversalRenderPipelineAsset) QualitySettings.renderPipeline;

			return m_cachedRenderPipeline;
		}
	}

	[SerializeField] public Sprite reticleSpriteFilled;
	[SerializeField] public Sprite reticleSpriteEmpty;

	[Header("Avatars prefabs used")]
	[SerializeField] public List<GameObject> avatarMalePrefabs = new List<GameObject>();
	[SerializeField] public List<GameObject> avatarFemalePrefabs = new List<GameObject>();

	[Header("Settings categories")]
	[SerializeField] public AvatarSettings avatarSettings = new AvatarSettings();
	[SerializeField] public AudioSettings audioSettings = new AudioSettings();
	[SerializeField] public GraphicsSettings graphicsSettings = new GraphicsSettings();
	[SerializeField] public GeneralSettings generalSettings = new GeneralSettings();
	[SerializeField] public RoleSettings roleSettings = new RoleSettings();
	[SerializeField] public OffsetSettings offsetSettings = new OffsetSettings();
	[SerializeField] public bool isLogEnabled = true;

	[SerializeField] public string ipAddress;

	public bool settingsInitializedFromFile = false;
	public string saveFile = "SaveConfig.dat";

	/// <summary>
	/// Settings version string is used to invalidate settings file when reading it, it can be virtually anything.
	/// Can be used to reset settings to default values.
	/// </summary>
	private string settingsVersion = "1";

	void Awake() {
		Debug.Log(Screen.brightness);
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

		// fast solution, would be better to rework it to use custom logger, so that we can control better what is / isn't logged
		// https://gamedevbeginner.com/how-to-use-debug-log-in-unity-without-affecting-performance/
		Debug.unityLogger.logEnabled = isLogEnabled;
	}

	void Start() {
		Application.targetFrameRate = 60;

		loadSettings();
	}

	void OnApplicationQuit() {
		saveSettings();
	}

	private void saveSettings() {
		SaveData sd = new SaveData();
		Instance.PopulateSaveData(sd);

		if (FileManager.WriteToFile(saveFile, sd.ToJson())) {
			Debug.Log("Succesfully saved data");
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
				settingsInitializedFromFile = true;

				initializeSettings();
			}

		}
	}

	private void initializeSettings() {
		SetRenderScale(graphicsSettings.renderScale);
		NetworkManager.singleton.networkAddress = ipAddress;
	}

	public void PopulateSaveData(SaveData saveData) {
		saveData.AudioSettings = audioSettings;
		saveData.AvatarSettings = avatarSettings;
		saveData.RoleSettings = roleSettings;
		saveData.OffsetSettings = offsetSettings;
		saveData.GraphicsSettings = graphicsSettings;
		saveData.GeneralSettings = generalSettings;
		saveData.IpAddress = ipAddress;
		saveData.SettingsVersion = settingsVersion;
	}

	public void LoadFromSaveData(SaveData saveData) {
		audioSettings = saveData.AudioSettings;
		avatarSettings = saveData.AvatarSettings;
		roleSettings = saveData.RoleSettings;
		offsetSettings = saveData.OffsetSettings;
		graphicsSettings = saveData.GraphicsSettings;
		generalSettings = saveData.GeneralSettings;
		ipAddress = saveData.IpAddress;
		settingsVersion = saveData.SettingsVersion;
	}

	public float currentRenderScale {
		get {
			VerifyCachedRenderPipeline ();
			if (CachedRenderPipeline == null) return -1;
			return CachedRenderPipeline.renderScale;
		}
	}

	public void SetRenderScale (float value) {
		VerifyCachedRenderPipeline ();
		if (CachedRenderPipeline == null) {
			Debug.LogError ("[QualityWrapper](SetRenderScale): Current Pipeline is null");
			return;
		}
		graphicsSettings.renderScale = Mathf.Clamp(value, 0.5f, 2);
		CachedRenderPipeline.renderScale = graphicsSettings.renderScale;
	}

	private void VerifyCachedRenderPipeline () {
		if ((UniversalRenderPipelineAsset) QualitySettings.renderPipeline == null)
			return;

		if (CachedRenderPipeline != (UniversalRenderPipelineAsset) QualitySettings.renderPipeline) {
			m_cachedRenderPipeline = (UniversalRenderPipelineAsset) QualitySettings.renderPipeline;
		}
	}
}
