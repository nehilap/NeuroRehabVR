using UnityEngine;

/// <summary>
/// https://www.youtube.com/watch?v=uD7y4T4PVk0
/// </summary>
[System.Serializable]
public class SaveData {
	[SerializeField] private AvatarSettings avatarSettings;
	[SerializeField] private AudioSettings audioSettings;
	[SerializeField] private GraphicsSettings graphicsSettings;
	[SerializeField] private GeneralSettings generalSettings;
	[SerializeField] private RoleSettings roleSettings;
	[SerializeField] private OffsetSettings offsetSettings;
	[SerializeField] private string ipAddress;

	[SerializeField] private string settingsVersion;

	public AudioSettings AudioSettings { get => audioSettings; set => audioSettings = value; }
	public AvatarSettings AvatarSettings { get => avatarSettings; set => avatarSettings = value; }
	public GraphicsSettings GraphicsSettings { get => graphicsSettings; set => graphicsSettings = value; }
	public RoleSettings RoleSettings { get => roleSettings; set => roleSettings = value; }
	public OffsetSettings OffsetSettings { get => offsetSettings; set => offsetSettings = value; }
	public GeneralSettings GeneralSettings { get => generalSettings; set => generalSettings = value; }
	public string IpAddress { get => ipAddress; set => ipAddress = value; }
	public string SettingsVersion { get => settingsVersion; set => settingsVersion = value; }

	public string ToJson() {
		return JsonUtility.ToJson(this);
	}

	public void LoadFromJson(string json) {
		JsonUtility.FromJsonOverwrite(json, this);
	}
}

public interface ISaveable {
	void PopulateSaveData(SaveData saveData);
	void LoadFromSaveData(SaveData saveData);
}