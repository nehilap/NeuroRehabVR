using UnityEngine.InputSystem;
using UnityEngine;

/// <summary>
/// Component used to animate avatar using premade animations. Supports both keyboard/controller input and head movements. Prioritizes direct input over head movement.
/// </summary>
public class AvatarWalkingController : MonoBehaviour {
	[SerializeField] private Animator animator;

	[Header("Input movement (controller / keyboard)")]
	[SerializeField] private InputActionReference move;

	[Header("Head movement")]
	[SerializeField] private InputActionReference headMove;
	[SerializeField] private Transform cameraTransform;
	[Range(0.1f, 4f)] [SerializeField] private float headMoveDuration = 0.7f;
	[Range(0.001f, 0.4f)] [SerializeField] private float headMoveTreshold = 0.12f;

	private bool isAnimatingLegs = false;
	private bool isAnimatingHead = false;
	private Vector3 lastHeadPosition = Vector3.zero;
	private float lastHeadMovementTime;

	private void OnEnable() {
		move.action.performed += animateLegsAction;
		move.action.canceled += stopAnimateLegsAction;

		if (headMove) {
			headMove.action.performed += animateHeadMovement;
			lastHeadPosition = cameraTransform.position;
		}
	}

	private void OnDisable() {
		move.action.performed -= animateLegsAction;
		move.action.canceled -= stopAnimateLegsAction;

		if (headMove) {
			headMove.action.performed -= animateHeadMovement;
		}
	}

	private void Update() {
		if (isAnimatingLegs) {
			return;
		}
		if (!isAnimatingHead) {
			return;
		}

		if ((Time.time - lastHeadMovementTime) > headMoveDuration) {
			stopAnimateLegs();
			isAnimatingHead = false;
		}
	}

	private void animateHeadMovement(InputAction.CallbackContext obj) {
		if (isAnimatingLegs) {
			return;
		}

		Vector3 headPosition = cameraTransform.position;
		Vector3 positionDiff = headPosition - lastHeadPosition;

		if (Mathf.Abs(positionDiff.x) < headMoveTreshold && Mathf.Abs(positionDiff.z) < headMoveTreshold) {
			return;
		}

		Vector3 direction = cameraTransform.InverseTransformDirection(positionDiff);

		lastHeadPosition = headPosition;
		lastHeadMovementTime = Time.time;

		isAnimatingHead = true;
		handleMovement(new Vector2(direction.x, direction.z));
	}

	/// <summary>
	/// Event used to set Animator variables
	/// </summary>
	/// <param name="obj"></param>
	private void animateLegsAction(InputAction.CallbackContext obj) {
		isAnimatingLegs = true;
		handleMovement(move.action.ReadValue<Vector2>());
	}

	private void handleMovement(Vector2 movementVector) {
		bool isWalking = movementVector.y != 0;
		bool isStrafing = movementVector.x != 0;

		if (Mathf.Abs(movementVector.y) < Mathf.Abs(movementVector.x)) {
			if (isWalking) isWalking = false;
		}

		movementVector.y = movementVector.y > 0f ? 1f : movementVector.y < 0f ? -1f : 0f;
		movementVector.x = movementVector.x > 0f ? 1f : movementVector.x < 0f ? -1f : 0f;

		if (isWalking) {
			animator.SetBool("isWalking", true);

			animator.SetBool("isStrafingRight", false);
			animator.SetBool("isStrafingLeft", false);

			animator.SetFloat("animationSpeed", movementVector.y);
		} else if (isStrafing) {
			animator.SetBool("isWalking", false);
			animator.SetFloat("strafeSpeed", 1);

			animator.SetBool("isStrafingRight", movementVector.x == 1);
			animator.SetBool("isStrafingLeft", movementVector.x == -1);
		}
	}

	private void stopAnimateLegsAction(InputAction.CallbackContext obj) {
		isAnimatingLegs = false;
		stopAnimateLegs();

		if (headMove) {
			lastHeadPosition = cameraTransform.position;
		}
	}

	private void stopAnimateLegs() {
		animator.SetBool("isWalking", false);
		animator.SetFloat("animationSpeed", 0);

		animator.SetBool("isStrafingRight", false);
		animator.SetBool("isStrafingLeft", false);
		animator.SetFloat("strafeSpeed", 0);
	}
}
