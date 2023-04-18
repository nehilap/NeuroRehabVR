using UnityEngine;
using Mirror;
using System.Collections.Generic;
using Enums;

/// <summary>
/// Message manager that contains list of all statusMessage objects, used to show/hide messages.
/// </summary>
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

	[SerializeField] private List<StatusMessage> statusMessageElements = new List<StatusMessage>();

	private void Start() {
		List<GameObject> statusMessageObjects = ObjectManager.Instance.getObjectsByName("StatusMessage");
		foreach (GameObject statusMessageObject in statusMessageObjects) {
			if (statusMessageObject.TryGetComponent<StatusMessage>(out StatusMessage smm)) {
				statusMessageElements.Add(smm);
			}
		}
	}

	[ClientRpc]
	public void RpcInformClients(string message, MessageType messageType) {
		showMessage(message, messageType);
	}

	[Client]
	public void showMessage(string message, MessageType messageType) {
		foreach (StatusMessage statusMessageManager in statusMessageElements) {
			statusMessageManager.showMessage(message, messageType);
		}
	}

	[Client]
	public void hideMessage() {
		foreach (StatusMessage statusMessageManager in statusMessageElements) {
			statusMessageManager.hideMessage();
		}
	}
}
