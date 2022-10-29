using UnityEngine;
using Mirror;

public class TherapistMenuManager : NetworkBehaviour
{
	[SyncVar]
	public NetworkIdentity patientIdentity;

	public void setupPatientIdentity(NetworkIdentity _patient) {
		patientIdentity = _patient;
		Debug.Log("Patient succesfully setup in therapis menu");
		CMDUpdatePatientIdentity(_patient);
	}

	public void playAnimationShowcaseHandler() {
		CharacterManager.localCharacter.CmdStartAnimationShowcase(patientIdentity);
	}

	public void playAnimationHandler() {
		CharacterManager.localCharacter.CmdStartAnimation(patientIdentity);
	}

	public void stopAnimationHandler() {
		CharacterManager.localCharacter.CmdStopAnimation(patientIdentity);
	}

	[Command(requiresAuthority = false)]
	public void CMDUpdatePatientIdentity(NetworkIdentity identity) {
		patientIdentity = identity;
	}
}
