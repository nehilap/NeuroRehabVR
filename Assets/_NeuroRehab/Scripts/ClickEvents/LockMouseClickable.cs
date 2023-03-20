using UnityEngine;
using Utility;

public class LockMouseClickable : MonoBehaviour, MouseClickable
{
	public void OnMouseClicked() {
		if (NetworkCharacterManager.localNetworkClientInstance != null) {
			Transform customTarget = gameObject.GetComponent<TargetUtility>().customTargetPos.transform;

			NetworkCharacterManager.localNetworkClientInstance.CmdSetLockPosition(new Mappings.PosRotMapping(customTarget));
		}
	}
}
