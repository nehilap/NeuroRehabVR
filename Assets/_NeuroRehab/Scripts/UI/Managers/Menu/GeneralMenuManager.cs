using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Enums;

public class GeneralMenuManager : MonoBehaviour {
	private JoinManager joinManager;

	void Start() {
		joinManager = new JoinManager();
	}

	public void JoinTherapist() {
		SettingsManager.Instance.roleSettings.createCharacter(UserRole.Therapist);
		joinManager.Join();
	}

	public void JoinPatient() {
		StartCoroutine(XRStatusManager.Instance.startXR());

		SettingsManager.Instance.roleSettings.createCharacter(UserRole.Patient);
		joinManager.Join();
	}

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
