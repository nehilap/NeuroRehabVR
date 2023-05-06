using TMPro;
using UnityEngine;
using System.IO;

public class FPSCounterManager : MonoBehaviour {
	[SerializeField] private TMP_Text textField;
	[SerializeField][Range(0.001f, 10f)] private float refreshRate = 1f;

	private float uiUpdateTime;
	public int frameRate { get; private set; }

	void OnApplicationQuit() {
		if (SettingsManager.Instance.generalSettings.measureFps && SettingsManager.Instance.generalSettings.writeFps && Application.platform != RuntimePlatform.Android) {
			System.Diagnostics.Process.Start(SettingsManager.Instance.generalSettings.fpsCounterFilePath);
		}
	}

	void Start() {
		textField = GetComponent<TMP_Text>();

		if (!SettingsManager.Instance.generalSettings.measureFps) {
			this.enabled = false;
		}

		StreamWriter writer = new StreamWriter(SettingsManager.Instance.generalSettings.fpsCounterFilePath, false);
		writer.WriteLine("");
		writer.Close();
	}

	private void OnDisable() {
		textField.text = "";
	}

	private void Update() {
		if (Time.unscaledTime > uiUpdateTime) {
			frameRate = (int)(1f / Time.unscaledDeltaTime);
			textField.text = $"FPS: {frameRate}";
			uiUpdateTime = Time.unscaledTime + refreshRate;
			if (SettingsManager.Instance.generalSettings.writeFps) {
				writeFps();
			}
		}
	}

	protected void OnGUI() {
		if (XRSettingsManager.Instance.isXRActive) return;
		if (!SettingsManager.Instance.generalSettings.measureFps) return;

		GUIStyle style = new();
		style.normal.textColor = Color.black;
		style.fontSize = 18;
		GUI.Label(new Rect(Screen.width - 80, 10, 70, 25), $"FPS: {frameRate}", style);
	}

	private void writeFps() {
		StreamWriter writer = new StreamWriter(SettingsManager.Instance.generalSettings.fpsCounterFilePath, true);
		writer.WriteLine($"{frameRate}");
		writer.Close();
	}
}
