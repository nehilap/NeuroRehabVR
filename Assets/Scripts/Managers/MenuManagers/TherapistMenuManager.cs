using UnityEngine;
using Mirror;

public class TherapistMenuManager : NetworkBehaviour
{
	public void playAnimationShowcaseHandler() {
		CharacterManager.localCharacter.CmdStartAnimationShowcase();
	}

	public void playAnimationHandler() {
		CharacterManager.localCharacter.CmdStartAnimation();
	}

	public void stopAnimationHandler() {
		CharacterManager.localCharacter.CmdStopAnimation();
	}
}
