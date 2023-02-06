using Mirror;
using UnityEngine;
using System.Collections.Generic;
using Enums;

public class DesktopCharacterManager : CharacterManager {
	

	[Header("Desktop Character Manager")]
	//[Header("Spawn sync vars")]

	//[Header("Run sync vars")]

	//[Header("Avatars prefabs used")]

	//[Header("Activated objects based on 'authority'")]

	//[Header("Camera culling and objects to disable")]
	[Header("Components to disable")]
	[SerializeField] private MonoBehaviour[] componentsToDisable;

	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();
	}

	public override void OnStopClient() {
		base.OnStopClient();
	}

	public new void Start() {
		base.Start();

		if (!isLocalPlayer) {
			foreach (MonoBehaviour component in componentsToDisable) {
				component.enabled = false;	
			}
		}
	}
}
