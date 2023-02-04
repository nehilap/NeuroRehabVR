using UnityEngine;

public class AudioSourceManager : MonoBehaviour {
    private static AudioSourceManager _instance;

    public static AudioSourceManager Instance { get { return _instance; } }


    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }
}
