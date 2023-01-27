using UnityEngine;

[System.Serializable]
public class AvatarSettings {
    public bool isFemale;
    public int avatarNumber;
    public float sizeMultiplier;
    public GameObject currentModel;
}

public class SettingsManager : MonoBehaviour {
    public static SettingsManager instance;

    [SerializeField]
    public AvatarSettings avatarSettings;

    void Start() {
        if (instance == null) {
            instance = this;
        }

        Application.targetFrameRate = 60;
	}
}
