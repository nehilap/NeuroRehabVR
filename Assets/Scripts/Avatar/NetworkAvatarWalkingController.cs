using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class NetworkAvatarWalkingController : NetworkBehaviour
{
	[SerializeField]
	private AvatarModelManager avatarModelManager;

	[SerializeField]
	private InputActionReference move;

	[SerializeField]
	private Animator maleAnimator;

	[SerializeField]
	private Animator femaleAnimator;

	[SerializeField]
	private Animator animator;

	[SerializeField] [SyncVar(hook = nameof(changeIsWalking))]
	private bool isWalking;

	[SerializeField] [SyncVar(hook = nameof(changeWalkingSpeed))]
	private float walkingSpeed;

	[SerializeField] [SyncVar(hook = nameof(changeIsStrafing))]
	private bool isStrafing;

	[SerializeField] [SyncVar(hook = nameof(changeStrafeSpeed))]
	private float strafeDirection;

	private void Start() {
		initAnimator();
	}

	private void OnEnable() {
		if (!isLocalPlayer) {
			return;
		}

		move.action.performed += updateStartAnimation;
		move.action.canceled += updateStopAnimation;
	}

	private void OnDisable() {
		if (!isLocalPlayer) {
			return;
		}
		move.action.performed -= updateStartAnimation;
		move.action.canceled -= updateStopAnimation;
	}

	private void initAnimator() {
		if (avatarModelManager.isFemale) {
			animator = femaleAnimator;
		} else {
			animator = maleAnimator;
		}
	}

	private void updateStartAnimation(InputAction.CallbackContext obj) {
		bool _isMoving = move.action.ReadValue<Vector2>().y != 0;
		bool _isStrafing = move.action.ReadValue<Vector2>().x != 0;

		isWalking = _isMoving;
		isStrafing = _isStrafing;
		if (_isMoving) {
			if (move.action.ReadValue<Vector2>().y > 0) {
				walkingSpeed = 1f; // front
			} else {
				walkingSpeed = -1f; // back
			}
		} else if (_isStrafing) {
			if (move.action.ReadValue<Vector2>().x > 0) {
				strafeDirection = 1f; // right
			} else {
				strafeDirection = -1f; // left
			}
		}

		CMDUpdateIsWalking(isWalking);
		CMDUpdateIsStrafing(isStrafing);
		CMDUpdateWalkingSpeed(walkingSpeed);
		CMDUpdateStrafeDirection(strafeDirection);
	}

	private void updateStopAnimation(InputAction.CallbackContext obj) {
		isWalking = false;
		isStrafing = false;
		strafeDirection = 0f;
		walkingSpeed = 0f;

		CMDUpdateIsWalking(isWalking);
		CMDUpdateIsStrafing(isStrafing);
		CMDUpdateWalkingSpeed(walkingSpeed);
		CMDUpdateStrafeDirection(strafeDirection);
	}

	private void animateLegs() {
		if (isLocalPlayer) {
			return;
		}
		if (animator == null) {
			initAnimator();
		}

		if (isWalking) {
			animator.SetBool("isWalking", true);
			
			animator.SetBool("isStrafingRight", false);
			animator.SetBool("isStrafingLeft", false);

			if (walkingSpeed == 1f) {
				animator.SetFloat("animationSpeed", 1);
			} else if (walkingSpeed == -1f){
				animator.SetFloat("animationSpeed", -1);
			}
		} else if (isStrafing) {
			animator.SetBool("isWalking", false);
			if (strafeDirection == 1f) {
				animator.SetFloat("strafeSpeed", 1);
				animator.SetBool("isStrafingRight", true);
				animator.SetBool("isStrafingLeft", false);
			} else if (strafeDirection == -1f){
				animator.SetFloat("strafeSpeed", 1);
				animator.SetBool("isStrafingLeft", true);
				animator.SetBool("isStrafingRight", false);
			}
		} else if (isWalking && isStrafing) {
			animator.SetBool("isWalking", true);

			animator.SetBool("isStrafingLeft", false);
			animator.SetBool("isStrafingRight", false);
			if (walkingSpeed == 1f) {
				animator.SetFloat("animationSpeed", 1);
			} else if (walkingSpeed == -1f){
				animator.SetFloat("animationSpeed", -1);
			}
		}
	}

	private void stopAnimateLegs() {
		if (isLocalPlayer) {
			return;
		}
		
		if (animator == null) {
			initAnimator();
		}


		if (!isWalking) {
			animator.SetBool("isWalking", false);
			animator.SetFloat("animationSpeed", 0);
		}

		if (!isStrafing) {
			animator.SetBool("isStrafingRight", false);
			animator.SetBool("isStrafingLeft", false);
			animator.SetFloat("strafeSpeed", 0);
		}
		animateLegs();
	}

	private void changeIsWalking(bool _old, bool _new) {
		if (_new == true) {
			animateLegs();
		} else {
			stopAnimateLegs();
		}
	}

	private void changeWalkingSpeed(float _old, float _new) {
		if (_new != _old) {
			animateLegs();
		}
	}

	private void changeIsStrafing(bool _old, bool _new) {
		if (_new == true) {
			animateLegs();
		} else {
			stopAnimateLegs();
		}
	}

	private void changeStrafeSpeed(float _old, float _new) {
		if (_new != _old) {
			animateLegs();
		}
	}
	
	[Command]
	public void CMDUpdateIsWalking(bool _isWalking) {
		isWalking = _isWalking;
	}

	[Command]
	public void CMDUpdateIsStrafing(bool _isStrafing) {
		isStrafing = _isStrafing;
	}
	[Command]
	public void CMDUpdateWalkingSpeed(float _walkingSpeed) {
		walkingSpeed = _walkingSpeed;
	}
	[Command]
	public void CMDUpdateStrafeDirection(float _direction) {
		strafeDirection = _direction;
	}
}
