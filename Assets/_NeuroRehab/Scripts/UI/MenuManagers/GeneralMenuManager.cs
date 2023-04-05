using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Enums;

public class GeneralMenuManager : MonoBehaviour {

	public string targetAPI;

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
	/*
	public void SendSync() => StartCoroutine(PostData_Coroutine());

	IEnumerator PostData_Coroutine()
	{
		if (targetAPI == "") {
			targetAPI = "https://8f15f933-34cb-4124-a097-d0a0c0b82f95.mock.pstmn.io";
		}
		WWWForm form = new WWWForm();
		using(UnityWebRequest request = UnityWebRequest.Post(targetAPI + "/sync", form))
		{
			yield return request.SendWebRequest();
			if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
				Debug.LogError("Failed to retrieve response from server!!");
			else
				Debug.Log("POST request received, message: \"" + request.downloadHandler.text + "\"");
		}
	}
	*/
}
