using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Custom character controller, that allows us to offset character controller, because default implementation does not allow that. We use headCollider, which is offset based on camera rotation. We use this workaround, since you can move camera and xrOrigin independently.
/// </summary>
public class CustomCharacterControllerDriver : CharacterControllerDriver {

	[Tooltip("Used for determining correct center position for collider. REQUIRED!!")]
	[SerializeField] private Transform headCollider;

	/// <summary>
	/// Used to correctly move center of character controller, based on camera rotation
	/// </summary>
	protected override void UpdateCharacterController() {
		try {
			if (xrOrigin == null || characterController == null) {
				return;
			}

			// Vector3 center = xrOrigin.CameraInOriginSpacePos + xrOrigin.Camera.transform.right * centerOffset.x + xrOrigin.Camera.transform.up * centerOffset.y + xrOrigin.Camera.transform.forward * centerOffset.z;
			var height = Mathf.Clamp(xrOrigin.CameraInOriginSpaceHeight, minHeight, maxHeight);

			// We have to transform it twice, because both camera and xr origin can move independently from each other.... That's why we use head collider
			// camera local -> world -> xrOrigin local
			Vector3 center = characterController.transform.InverseTransformPoint(headCollider.TransformPoint(headCollider.GetComponent<CapsuleCollider>().center));
			center.y = height / 2f + characterController.skinWidth;

			characterController.height = height;
			characterController.center = center;
		} catch (System.Exception) {
			base.UpdateCharacterController();
		}
	}
}
