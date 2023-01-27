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

	public void setAnimationStartPositionHandler() {
		NetworkCharacterManager.localNetworkClient.CmdSetAnimationStartPosition();
	}

	public void setAnimationEndPositionHandler() {
		NetworkCharacterManager.localNetworkClient.CmdAddMovePosition();
	}

	public void clearAnimationEndPositionHandler() {
		NetworkCharacterManager.localNetworkClient.CmdClearAnimationMovePositions();
	}
}
