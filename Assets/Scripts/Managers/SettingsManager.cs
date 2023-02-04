using UnityEngine;

[System.Serializable]
public class AvatarSettings {
    public bool isFemale;
    public int avatarNumber;
    public float sizeMultiplier;
    public GameObject currentModel;
}

public class SettingsManager : MonoBehaviour {
   
    private static SettingsManager _instance;
    public static SettingsManager Instance { get { return _instance; } }

    [SerializeField] public AvatarSettings avatarSettings = new AvatarSettings();

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    void Start() {
        Application.targetFrameRate = 60;
	}
}
