using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TargetDraggable : NetworkBehaviour, DragInterface, TargetDisableInterface {

	private Rigidbody _rigidbody;
	public bool itemPickedUp = false;

	private XRGrabInteractable XRGrabInteractable;

	private void Awake() {
		_rigidbody = transform.GetComponent<Rigidbody>();
		XRGrabInteractable = gameObject.GetComponent<XRGrabInteractable>();
	}

	public void OnStartDrag() {
		_rigidbody.useGravity = false;
	}

	public void OnStopDrag() {
		_rigidbody.useGravity = true;
		_rigidbody.velocity = Vector3.zero;
	}

	public void OnShowDragRange() {
		if (CharacterManager.activePatientInstance != null) {
			CharacterManager.activePatientInstance.showArmRangeMarker();
		}
	}

	public void OnHideDragRange() {
		if (CharacterManager.activePatientInstance != null) {
			CharacterManager.activePatientInstance.hideArmRangeMarker();
		}
	}

	[Command]
	public void CmdDisableDrag() {
		itemPickedUp = true;
		RpcDisableDrag();
	}

	[ClientRpc]
	public void RpcDisableDrag() {
		// Debug.Log($"{gameObject.name} __ {CharacterManager.localClientInstance.netId} __ {netIdentity.isOwned}");
		if (!netIdentity.isOwned) {
			gameObject.tag = "Untagged";
			XRGrabInteractable.enabled = false;
		}
	}

	[Command]
	public void CmdEnableDrag() {
		enableDragWrapper();
	}

	[Server]
	public void enableDragWrapper() {
		itemPickedUp = false;
		RpcEnableDrag();
	}

	[ClientRpc]
	public void RpcEnableDrag() {
		if (!netIdentity.isOwned) {
			gameObject.tag = "Draggable";
			XRGrabInteractable.enabled = true;
		}
	}
}
