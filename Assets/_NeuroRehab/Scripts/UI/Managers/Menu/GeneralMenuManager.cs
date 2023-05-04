using UnityEngine;

public class GeneralMenuManager : MonoBehaviour {
	public void QuitApp() {
		Application.Quit();
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}

	public void triggerActive(GameObject _object) {
		_object.SetActive(!_object.activeSelf);
	}
}
