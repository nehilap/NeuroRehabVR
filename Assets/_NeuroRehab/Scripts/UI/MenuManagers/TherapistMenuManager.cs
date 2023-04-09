using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TherapistMenuManager : MonoBehaviour {
	[SerializeField] public List<CanvasGroup> animationCanvases = new List<CanvasGroup>();

	public void playAnimationShowcaseHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdStartAnimationShowcase();
	}

	public void playAnimationHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdStartTraining();
	}

	public void stopAnimationHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdStopTraining();
	}

	public void setArmRestHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		NetworkCharacterManager.localNetworkClientInstance.CmdSetArmRestPosition(CharacterManager.activePatientInstance.GetComponent<NetworkIdentity>());
	}

	public void sitPatientHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		NetworkCharacterManager.localNetworkClientInstance.CmdMovePatientToSit(CharacterManager.activePatientInstance.GetComponent<NetworkIdentity>());
	}

	public void sitAcrossTableHandler() {
		GameObject therapistSitPosition = ObjectManager.Instance.getFirstObjectByName("TherapistSitPositionObject");
		GameObject tableObject = ObjectManager.Instance.getFirstObjectByName("Table");

		CharacterManager.localClientInstance.teleportCharacter(therapistSitPosition.transform, tableObject.transform);
	}

	public void setAnimationStartPositionHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdSetAnimationStartPosition();
	}

	public void setAnimationEndPositionHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdAddMovePosition();
	}

	public void clearAnimationEndPositionHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdClearAnimationMovePositions();
	}

	public void moveTableUpHandler() {
		NetworkIdentity netId = CharacterManager.localClientInstance.gameObject.GetComponent<NetworkIdentity>();

		NetworkCharacterManager.localNetworkClientInstance.CmdMoveTable(new Vector3(0f, 0.02f, 0f), netId);
	}

	public void moveTableDownHandler() {
		NetworkIdentity netId = CharacterManager.localClientInstance.gameObject.GetComponent<NetworkIdentity>();

		NetworkCharacterManager.localNetworkClientInstance.CmdMoveTable(new Vector3(0f, -0.02f, 0f), netId);
	}

	public void movePatientForwardHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}

		NetworkIdentity patientId = CharacterManager.activePatientInstance.gameObject.GetComponent<NetworkIdentity>();

		NetworkCharacterManager.localNetworkClientInstance.CmdMovePatient(new Vector3(0f, 0f, 0.02f), patientId);
	}

	public void movePatientBackwardsHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}

		NetworkIdentity patientId = CharacterManager.activePatientInstance.gameObject.GetComponent<NetworkIdentity>();

		NetworkCharacterManager.localNetworkClientInstance.CmdMovePatient(new Vector3(0f, 0f, -0.02f), patientId);
	}

	public void movePatientRightHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}

		NetworkIdentity patientId = CharacterManager.activePatientInstance.gameObject.GetComponent<NetworkIdentity>();

		NetworkCharacterManager.localNetworkClientInstance.CmdMovePatient(new Vector3(0.02f, 0f, 0f), patientId);
	}

	public void movePatientLeftHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}

		NetworkIdentity patientId = CharacterManager.activePatientInstance.gameObject.GetComponent<NetworkIdentity>();

		NetworkCharacterManager.localNetworkClientInstance.CmdMovePatient(new Vector3(-0.02f, 0f, 0f), patientId);
	}

	public void setPatientAnimatedArm(bool isLeft) {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		NetworkIdentity patientId = CharacterManager.activePatientInstance.gameObject.GetComponent<NetworkIdentity>();

		NetworkCharacterManager.localNetworkClientInstance.CmdSetActiveArm(isLeft, patientId);
	}
}
