using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Enums;

public class MessageManager : NetworkBehaviour {

	private static MessageManager _instance;
	public static MessageManager Instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<MessageManager>();
			}
			return _instance;
		}
	}

	[SerializeField] private List<StatusMessageManager> statusMessageManagers = new List<StatusMessageManager>();

	private void Start() {
		List<GameObject> statusMessageObjects = ObjectManager.Instance.getObjectsByName("StatusMessage");
		foreach (GameObject statusMessageObject in statusMessageObjects) {
			if (statusMessageObject.TryGetComponent<StatusMessageManager>(out StatusMessageManager smm)) {
				statusMessageManagers.Add(smm);
			}
		}
	}


	[ClientRpc]
	public void RpcInformClients(string message, MessageType messageType) {
		showMessage(message, messageType);
	}

	public void showMessage(string message, MessageType messageType) {
		foreach (StatusMessageManager statusMessageManager in statusMessageManagers) {
			statusMessageManager.showMessage(message, messageType);
		}
	}

	public void hideMessage() {
		foreach (StatusMessageManager statusMessageManager in statusMessageManagers) {
			statusMessageManager.hideMessage();
		}
	}
}
