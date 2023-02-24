using UnityEngine;

public class AvatarLowerBodyAnimationController : MonoBehaviour {
	[SerializeField]
	private Animator animator;


	[SerializeField] [Range(0f, 1f)]
	private float leftFootPositionWeight;
	[SerializeField] [Range(0f, 1f)]
	private float rightFootPositionWeight;

	[SerializeField] [Range(0f, 1f)]
	private float leftFootRotationWeight;
	[SerializeField] [Range(0f, 1f)]
	private float rightFootRotationWeight;

	[SerializeField] private Vector3 footOffset;
	private Vector3 internalFootOffset;

	[SerializeField] private Vector3 raycastLeftOffset;
	private Vector3 internalRaycastLeftOffset;
	
	[SerializeField] private Vector3 raycastRightOffset;
	private Vector3 internalRaycastRightOffset;

	[SerializeField] private LayerMask groundLayer;

	[SerializeField] private Transform offsetTransform;
	[SerializeField] public float offsetDistance;
	public bool offsetPreset;

	[SerializeField] private AvatarController avatarController;

	private float groundOffset;

	private void OnEnable() {
		if (!offsetPreset) {
			offsetDistance = offsetTransform.TransformPoint(Vector3.zero).y;
		}

		SettingsManager.Instance.avatarSettings.offsetDistance = offsetDistance;
		internalFootOffset = footOffset * avatarController.sizeMultiplier;
		internalRaycastLeftOffset = raycastLeftOffset * avatarController.sizeMultiplier;
		internalRaycastRightOffset = raycastRightOffset * avatarController.sizeMultiplier;
		groundOffset = 0.045f * avatarController.sizeMultiplier;
	}

	private void OnAnimatorIK(int layerIndex) {
		Vector3 leftFootPosition = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
		Vector3 rightFootPosition = animator.GetIKPosition(AvatarIKGoal.RightFoot);

		RaycastHit leftFootHit;
		RaycastHit rightFootHit;

		bool isLeftFootDown = Physics.Raycast(leftFootPosition + raycastLeftOffset, Vector3.down, out leftFootHit);
		bool isRightFootDown = Physics.Raycast(rightFootPosition + raycastRightOffset, Vector3.down, out rightFootHit);

		bool isCrouching = Physics.Raycast(offsetTransform.position, Vector3.down, offsetDistance - groundOffset, groundLayer.value);

		if (isCrouching) {
			calculateFoot(isLeftFootDown, leftFootHit, AvatarIKGoal.LeftFoot, leftFootPositionWeight, leftFootRotationWeight);
			calculateFoot(isRightFootDown, rightFootHit, AvatarIKGoal.RightFoot, rightFootPositionWeight, rightFootRotationWeight);
		}
	}

	private void calculateFoot(bool isFootDown, RaycastHit footHit, AvatarIKGoal goal, float footPositionWeight, float footRotationWeight) {
		if(isFootDown) {
			animator.SetIKPositionWeight(goal, footPositionWeight);
			animator.SetIKPosition(goal, footHit.point + footOffset);

			Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, footHit.normal), footHit.normal);
			animator.SetIKRotationWeight(goal, footRotationWeight);
			animator.SetIKRotation(goal, footRotation);
		} else {
			animator.SetIKPositionWeight(goal, 0f);
		}
	}
}
