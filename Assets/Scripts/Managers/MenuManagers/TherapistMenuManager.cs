using Mirror;
using UnityEngine;

public class TherapistMenuManager : MonoBehaviour {
	public void playAnimationShowcaseHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdStartAnimationShowcase();
	}

	public void playAnimationHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdStartAnimation();
	}

	public void stopAnimationHandler() {
		NetworkCharacterManager.localNetworkClientInstance.CmdStopAnimation();
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
		if (ObjectManager.Instance.getFirstObjectByName("TherapistSitPositionObject") != null) {
			// We have to turn off character controller, as it stops us trying to teleport object around
			CharacterController cc = CharacterManager.localClientInstance.GetComponent<CharacterController>();

			cc.enabled = false;
			CharacterManager.localClientInstance.transform.position = ObjectManager.Instance.getFirstObjectByName("TherapistSitPositionObject").transform.position;

			CharacterManager.localClientInstance.transform.LookAt(ObjectManager.Instance.getFirstObjectByName("Table").transform);
			float cameraRot = CharacterManager.localClientInstance.cameraObject.transform.localRotation.eulerAngles.y;

			CharacterManager.localClientInstance.transform.rotation = Quaternion.Euler(CharacterManager.localClientInstance.transform.eulerAngles.x, CharacterManager.localClientInstance.transform.eulerAngles.y - cameraRot, CharacterManager.localClientInstance.transform.eulerAngles.z);
			cc.enabled = true;
		}
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
