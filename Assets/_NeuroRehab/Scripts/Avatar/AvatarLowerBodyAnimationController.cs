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

	private readonly float groundOffsetConst = 0.15f;

	private void OnEnable() {
		if (!offsetPreset) {
			offsetDistance = offsetTransform.TransformPoint(Vector3.zero).y;
		}

		resetValues();
	}

	public void resetHeight() {
		offsetDistance = offsetTransform.TransformPoint(Vector3.zero).y;
		resetValues();
	}

	private void resetValues() {
		/*
		Debug.Log("offset dist " + offsetDistance);
		Debug.Log(groundOffset);
		*/
		SettingsManager.Instance.avatarSettings.offsetDistance = offsetDistance;
		internalFootOffset = footOffset * avatarController.sizeMultiplier;
		internalRaycastLeftOffset = raycastLeftOffset * avatarController.sizeMultiplier;
		internalRaycastRightOffset = raycastRightOffset * avatarController.sizeMultiplier;
		groundOffset = groundOffsetConst * avatarController.sizeMultiplier;
	}

	// we use this to determine whether character is crouching or not
	// if yes, we move feet to be on the ground level so that avatar appears to be crouching
	private void OnAnimatorIK(int layerIndex) {
		bool isCrouching = Physics.Raycast(offsetTransform.position, Vector3.down, offsetDistance - groundOffset, groundLayer.value);

		if (!isCrouching) {
			return;
		}
		Vector3 leftFootPosition = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
		Vector3 rightFootPosition = animator.GetIKPosition(AvatarIKGoal.RightFoot);

		RaycastHit leftFootHit;
		RaycastHit rightFootHit;

		bool isLeftFootDown = Physics.Raycast(leftFootPosition + raycastLeftOffset, Vector3.down, out leftFootHit);
		bool isRightFootDown = Physics.Raycast(rightFootPosition + raycastRightOffset, Vector3.down, out rightFootHit);

		calculateFoot(isLeftFootDown, leftFootHit, AvatarIKGoal.LeftFoot, leftFootPositionWeight, leftFootRotationWeight);
		calculateFoot(isRightFootDown, rightFootHit, AvatarIKGoal.RightFoot, rightFootPositionWeight, rightFootRotationWeight);
	}

	private void calculateFoot(bool isFootDown, RaycastHit footHit, AvatarIKGoal goal, float footPositionWeight, float footRotationWeight) {
		if(!isFootDown) {
			animator.SetIKPositionWeight(goal, 0f);
			return;
		}

		animator.SetIKPositionWeight(goal, footPositionWeight);
		animator.SetIKPosition(goal, footHit.point + footOffset);

		Quaternion footRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, footHit.normal), footHit.normal);
		animator.SetIKRotationWeight(goal, footRotationWeight);
		animator.SetIKRotation(goal, footRotation);
	}
}
