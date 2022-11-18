using UnityEngine;
using Mirror;

public class TherapistMenuManager : NetworkBehaviour
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
		if (CharacterManager.activePatient == null) {
			Debug.LogError("No active patient found");
		}

		CharacterManager.localClient.CmdSetAnimationStartPosition();
	}

	public void setAnimationEndPositionHandler() {
		if (CharacterManager.activePatient == null) {
			Debug.LogError("No active patient found");
		}

		CharacterManager.localClient.CmdSetAnimationEndPosition();
	}

	public void clearAnimationEndPositionHandler() {
		if (CharacterManager.activePatient == null) {
			Debug.LogError("No active patient found");
		}

		CharacterManager.localClient.CmdClearAnimationEndPosition();
	}
}
