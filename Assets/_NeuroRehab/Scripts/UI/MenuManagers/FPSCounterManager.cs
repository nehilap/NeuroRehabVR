using TMPro;
using UnityEngine;

public class FPSCounterManager : MonoBehaviour {
	[SerializeField] private TMP_Text textField;
	[SerializeField][Range(0.001f, 10f)] private float refreshRate = 1f;

	private float uiUpdateTime;
	public int frameRate { get; private set; }

	void Start() {
		textField = GetComponent<TMP_Text>();
	}

	private void Update() {
		if (Time.unscaledTime > uiUpdateTime) {
			frameRate = (int)(1f / Time.unscaledDeltaTime);
			textField.text = $"FPS: {frameRate}";
			uiUpdateTime = Time.unscaledTime + refreshRate;
		}
	}

	protected void OnGUI() {
		if (XRStatusManager.Instance.isXRActive) return;
		GUIStyle style = new();
		style.normal.textColor = Color.black;
		style.fontSize = 18;
		GUI.Label(new Rect(Screen.width - 80, 10, 70, 25), $"FPS: {frameRate}", style);
	}
}
