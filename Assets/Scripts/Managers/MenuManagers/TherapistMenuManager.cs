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
}
