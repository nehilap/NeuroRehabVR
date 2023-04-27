using UnityEngine;
using UnityEngine.InputSystem;

public class StaticMenuManager : MiniMenuManager {

	[SerializeField] private Transform cameraTransform;

	[SerializeField] private float menuOffset = 1.25f;

	protected override void Awake() {
		base.Awake();
		transform.position = cameraTransform.position + (cameraTransform.forward * menuOffset);
		transform.localRotation = cameraTransform.localRotation;
	}

	protected override void Start() {
		base.Start();
	}

	protected override void renderMenu() {
		base.renderMenu();

		if (isMenuShowing) {
			transform.position = cameraTransform.position + (cameraTransform.forward * transformOffset.z);
			transform.localRotation = cameraTransform.localRotation;
			transform.SetParent(null);
		}
	}

	public override void offsetRight() {
		transformOffset += new Vector3(0.05f, 0f, 0f);
		transform.localPosition += transform.right * 0.05f;
	}

	public override void offsetLeft() {
		transformOffset += new Vector3(-0.05f, 0f, 0f);
		transform.localPosition += transform.right * -0.05f;
	}

	public override void offsetFwd() {
		transformOffset += new Vector3(0f, 0f, 0.05f);
		transform.localPosition += transform.forward * 0.05f;
	}

	public override void offsetBack() {
		transformOffset += new Vector3(0f, 0f, -0.05f);
		transform.localPosition += transform.forward * -0.05f;
	}
}
