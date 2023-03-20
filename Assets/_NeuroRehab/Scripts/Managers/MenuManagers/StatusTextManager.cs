using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class StatusTextManager : MonoBehaviour {

	private static StatusTextManager _instance;
	public static StatusTextManager Instance { get { return _instance; } }

	private void Awake() {
		if (_instance != null && _instance != this) {
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}
	void Start() {
		InitStatusText();
	}

	public void InitStatusText() {
		if(!this.gameObject.scene.isLoaded) return;
		
		GetComponent<TMP_Text>().text = "Platform: '" + Application.platform.ToString() 
			+ "', HMD type: '" + XRStatusManager.Instance.hmdType.ToString()
			+ "', Device: '" + XRSettings.loadedDeviceName + "'";
	}
}
