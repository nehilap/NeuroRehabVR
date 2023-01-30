using Mirror;
using UnityEngine;

public class TherapistMenuManager : MonoBehaviour {
	public void playAnimationShowcaseHandler() {
		NetworkCharacterManager.localNetworkClient.CmdStartAnimationShowcase();
	}

	public void playAnimationHandler() {
		NetworkCharacterManager.localNetworkClient.CmdStartAnimation();
	}

	public void stopAnimationHandler() {
		NetworkCharacterManager.localNetworkClient.CmdStopAnimation();
	}

	public void setArmRestHandler() {
		NetworkCharacterManager.localNetworkClient.CmdSetArmRestPosition(CharacterManager.activePatient.GetComponent<NetworkIdentity>());
	}

	public void setAnimationStartPositionHandler() {
		NetworkCharacterManager.localNetworkClient.CmdSetAnimationStartPosition();
	}

	public void setAnimationEndPositionHandler() {
		NetworkCharacterManager.localNetworkClient.CmdAddMovePosition();
	}

	public void clearAnimationEndPositionHandler() {
		NetworkCharacterManager.localNetworkClient.CmdClearAnimationMovePositions();
	}

	public void moveTableUpHandler() {
		NetworkIdentity netId = CharacterManager.localClient.gameObject.GetComponent<NetworkIdentity>();

		NetworkCharacterManager.localNetworkClient.CmdMoveTable(new Vector3(0f, 0.02f, 0f), netId);
	}
	
	public void moveTableDownHandler() {
		NetworkIdentity netId = CharacterManager.localClient.gameObject.GetComponent<NetworkIdentity>();

		NetworkCharacterManager.localNetworkClient.CmdMoveTable(new Vector3(0f, -0.02f, 0f), netId);
	}
}
