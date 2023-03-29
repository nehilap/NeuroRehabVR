using UnityEngine;

public class TargetDraggable : MonoBehaviour, DragInterface {

	private Rigidbody _rigidbody;

	private void Awake() {
		_rigidbody = transform.GetComponent<Rigidbody>();
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
}