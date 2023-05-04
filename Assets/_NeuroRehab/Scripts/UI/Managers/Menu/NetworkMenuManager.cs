using UnityEngine;
using Mirror;
using Enums;

public class NetworkMenuManager : MonoBehaviour {
	private JoinManager joinManager = new JoinManager();

	public void disconnectClient() {
		NetworkManager.singleton.StopClient();
	}

	public void JoinTherapist() {
		SettingsManager.Instance.roleSettings.createCharacter(UserRole.Therapist);
		joinManager.join();
	}

	public void JoinPatient() {
		StartCoroutine(XRSettingsManager.Instance.startXR());

		SettingsManager.Instance.roleSettings.createCharacter(UserRole.Patient);
		joinManager.join();
	}
}
