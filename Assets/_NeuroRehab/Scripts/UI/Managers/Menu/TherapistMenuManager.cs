using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Class containing handlers for UI elements - Therapist Menu
/// </summary>
public class TherapistMenuManager : MonoBehaviour {
	[SerializeField] public List<CanvasGroup> animationCanvases = new List<CanvasGroup>();

	public void playAnimationShowcaseHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdStartAnimationShowcase();
	}

	public void playAnimationHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdStartAnimation();
	}

	public void startTrainingHandler() {
		if (CharacterManager.activePatientInstance && CharacterManager.activePatientInstance.activeArmAnimationController.animState == NeuroRehab.Enums.AnimationState.Playing) {
			MessageManager.Instance.showMessage("Animation already running", NeuroRehab.Enums.MessageType.WARNING);
			return;
		}

		NetworkCharacterManager.localNetworkClientInstance.CmdStartTraining();
	}

	public void stopAnimationHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdStopTraining();
	}

	public void setArmRestHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		NetworkCharacterManager.localNetworkClientInstance.CmdSetArmRestPosition(CharacterManager.activePatientInstance.netIdentity);
	}

	public void sitPatientHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		NetworkCharacterManager.localNetworkClientInstance.CmdMovePatientToSit(CharacterManager.activePatientInstance.netIdentity);
	}

	public void sitAcrossTableHandler() {
		GameObject therapistSitPosition = ObjectManager.Instance.getFirstObjectByName("TherapistSitPositionObject");
		GameObject tableObject = ObjectManager.Instance.getFirstObjectByName("Table");

		CharacterManager.localClientInstance.teleportCharacter(therapistSitPosition.transform, tableObject.transform);
	}

	public void setAnimationStartPositionHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdSetAnimationStartPosition();
	}

	public void setAnimationMovePositionHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdAddMovePosition();
	}

	public void clearAnimationEndPositionHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdClearMovePositions();
	}

	public void deleteLastAnimationMovePositionHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdDeleteMovePosition();
	}

	public void moveTableUpHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdMoveTable(new Vector3(0f, 0.02f, 0f));
	}

	public void moveTableDownHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdMoveTable(new Vector3(0f, -0.02f, 0f));
	}

	public void movePatientForwardHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		NetworkIdentity patientId = CharacterManager.activePatientInstance.netIdentity;
		NetworkCharacterManager.localNetworkClientInstance.CmdMovePatient(new Vector3(0f, 0f, 0.02f), patientId);
	}

	public void movePatientBackwardsHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		NetworkIdentity patientId = CharacterManager.activePatientInstance.netIdentity;
		NetworkCharacterManager.localNetworkClientInstance.CmdMovePatient(new Vector3(0f, 0f, -0.02f), patientId);
	}

	public void movePatientRightHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		NetworkIdentity patientId = CharacterManager.activePatientInstance.netIdentity;
		NetworkCharacterManager.localNetworkClientInstance.CmdMovePatient(new Vector3(0.02f, 0f, 0f), patientId);
	}

	public void movePatientLeftHandler() {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		NetworkIdentity patientId = CharacterManager.activePatientInstance.netIdentity;
		NetworkCharacterManager.localNetworkClientInstance.CmdMovePatient(new Vector3(-0.02f, 0f, 0f), patientId);
	}

	public void moveArmRestForwardHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdMoveArmRest(new Vector3(0f, 0f, 0.02f));
	}

	public void moveArmRestBackwardsHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdMoveArmRest(new Vector3(0f, 0f, -0.02f));
	}

	public void moveArmRestRightHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdMoveArmRest(new Vector3(0.02f, 0f, 0f));
	}

	public void moveArmRestLeftHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdMoveArmRest(new Vector3(-0.02f, 0f, 0f));
	}


	public void setPatientAnimatedArm(bool isLeft) {
		if (CharacterManager.activePatientInstance == null) {
			return;
		}
		NetworkIdentity patientId = CharacterManager.activePatientInstance.netIdentity;
		NetworkCharacterManager.localNetworkClientInstance.CmdSetActiveArm(isLeft, patientId);
	}
}
