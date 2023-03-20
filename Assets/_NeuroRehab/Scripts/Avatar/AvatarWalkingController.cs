using UnityEngine.InputSystem;
using UnityEngine;

public class AvatarWalkingController : MonoBehaviour {
	[SerializeField] private InputActionReference move;

	[SerializeField] private Animator animator;

	private void OnEnable() {
		move.action.performed += animateLegs;
		move.action.canceled += stopAnimateLegs;
	}

	private void OnDisable() {
		move.action.performed -= animateLegs;
		move.action.canceled -= stopAnimateLegs;
	}

	/// <summary>
	/// Event used to set Animator variables
	/// </summary>
	/// <param name="obj"></param>
	private void animateLegs(InputAction.CallbackContext obj) {
		bool isMoving = move.action.ReadValue<Vector2>().y != 0;
		bool isStrafing = move.action.ReadValue<Vector2>().x != 0;

		if (Mathf.Abs(move.action.ReadValue<Vector2>().y) < Mathf.Abs(move.action.ReadValue<Vector2>().x)) {
			if (isMoving) isMoving = false;
		}

		if (isMoving) {
			animator.SetBool("isWalking", true);

			animator.SetBool("isStrafingRight", false);
			animator.SetBool("isStrafingLeft", false);

			if (move.action.ReadValue<Vector2>().y > 0) {
				animator.SetFloat("animationSpeed", 1);
			} else {
				animator.SetFloat("animationSpeed", -1);
			}
		} else if (isStrafing) {
			animator.SetBool("isWalking", false);
			if (move.action.ReadValue<Vector2>().x > 0) {
				animator.SetFloat("strafeSpeed", 1);
				animator.SetBool("isStrafingRight", true);
				animator.SetBool("isStrafingLeft", false);
			} else {
				animator.SetFloat("strafeSpeed", 1);
				animator.SetBool("isStrafingLeft", true);
				animator.SetBool("isStrafingRight", false);
			}
		} else if (isMoving && isStrafing) {
			animator.SetBool("isWalking", true);

			animator.SetBool("isStrafingLeft", false);
			animator.SetBool("isStrafingRight", false);
			if (move.action.ReadValue<Vector2>().y > 0) {
				animator.SetFloat("animationSpeed", 1);
			} else {
				animator.SetFloat("animationSpeed", -1);
			}
		}
	}

	private void stopAnimateLegs(InputAction.CallbackContext obj) {
		animator.SetBool("isWalking", false);
		animator.SetFloat("animationSpeed", 0);

		animator.SetBool("isStrafingRight", false);
		animator.SetBool("isStrafingLeft", false);
		animator.SetFloat("strafeSpeed", 0);
	}
}
