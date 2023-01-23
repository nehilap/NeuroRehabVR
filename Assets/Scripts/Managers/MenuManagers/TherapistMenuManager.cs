using UnityEngine;

public class TherapistMenuManager : MonoBehaviour
{
	public void playAnimationShowcaseHandler() {
		CharacterManager.localClient.CmdStartAnimationShowcase();
	}

	public void playAnimationHandler() {
		CharacterManager.localClient.CmdStartAnimation();
	}

	public void stopAnimationHandler() {
		CharacterManager.localClient.CmdStopAnimation();
	}

	public void setAnimationStartPositionHandler() {
		NetworkCharacterManager.localNetworkClient.CMDSetAnimationStartPosition(null);
	}

	public void setAnimationEndPositionHandler() {
		NetworkCharacterManager.localNetworkClient.CMDAddMovePosition();
	}

	public void clearAnimationEndPositionHandler() {
		NetworkCharacterManager.localNetworkClient.CMDClearAnimationMovePositions();
	}
}
