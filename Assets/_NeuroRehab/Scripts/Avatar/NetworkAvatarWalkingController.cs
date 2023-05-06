using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

/// <summary>
/// Same as 'AvatarWalkingController', but used to sync walking value over network, so that we can animate avatars on 'nonLocalPlayer'.
/// </summary>
public class NetworkAvatarWalkingController : NetworkBehaviour {

	[SerializeField] private AvatarModelManager avatarModelManager;

	[SerializeField] private Transform cameraTransform;
	[SerializeField] private InputActionReference headMove;
	[SerializeField] private InputActionReference move;

	[SerializeField] private Animator maleAnimator;

	[SerializeField] private Animator femaleAnimator;

	[SerializeField] private Animator animator;

	[SerializeField] [SyncVar(hook = nameof(changeIsWalking))]
	private bool isWalking;

	[SerializeField] [SyncVar(hook = nameof(changeWalkingSpeed))]
	private float walkingSpeed;

	[SerializeField] [SyncVar(hook = nameof(changeIsStrafing))]
	private bool isStrafing;

	[SerializeField] [SyncVar(hook = nameof(changeStrafeSpeed))]
	private float strafeDirection;

	private bool isAnimatingLegs = false;
	private bool isAnimatingHead = false;
	private Vector3 lastHeadPosition = Vector3.zero;
	private float lastHeadMovementTime;

	[Range(0.1f, 4f)] [SerializeField] private float headMoveDuration = 0.7f;
	[Range(0.001f, 0.2f)] [SerializeField] private float headMoveTreshold = 0.07f;

	private void Start() {
		initAnimator();
	}

	private void OnEnable() {
		if (!isLocalPlayer) {
			return;
		}

		move.action.performed += updateStartAnimation;
		move.action.canceled += updateStopAnimation;

		if (headMove) {
			headMove.action.performed += animateHeadMovement;
			lastHeadPosition = cameraTransform.position;
		}
	}

	private void OnDisable() {
		if (!isLocalPlayer) {
			return;
		}
		move.action.performed -= updateStartAnimation;
		move.action.canceled -= updateStopAnimation;

		if (headMove) {
			headMove.action.performed += animateHeadMovement;
		}
	}

	private void initAnimator() {
		if (avatarModelManager.isFemale) {
			animator = femaleAnimator;
		} else {
			animator = maleAnimator;
		}
	}

	private void Update() {
		if (!isLocalPlayer) {
			return;
		}
		if (isAnimatingLegs) {
			return;
		}
		if (!isAnimatingHead) {
			return;
		}

		if ((Time.time - lastHeadMovementTime) > headMoveDuration) {
			isWalking = false;
			isStrafing = false;
			isAnimatingHead = false;

			CMDUpdateIsWalking(isWalking);
			CMDUpdateIsStrafing(isStrafing);
		}
	}

	private void animateHeadMovement(InputAction.CallbackContext obj) {
		if (CharacterManager.localClientInstance != null && CharacterManager.localClientInstance.isArmResting) {
			return;
		}
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

		CMDUpdateIsWalking(isWalking);
		CMDUpdateIsStrafing(isStrafing);
		CMDUpdateWalkingSpeed(walkingSpeed);
		CMDUpdateStrafeDirection(strafeDirection);
	}

	/// <summary>
	/// Event called when movement is done, updates SyncVars on Server
	/// </summary>
	/// <param name="obj"></param>
	private void updateStartAnimation(InputAction.CallbackContext obj) {
		handleMovement(move.action.ReadValue<Vector2>());
		if (isWalking || isStrafing) {
			isAnimatingLegs = true;
		}

		CMDUpdateIsWalking(isWalking);
		CMDUpdateIsStrafing(isStrafing);
		CMDUpdateWalkingSpeed(walkingSpeed);
		CMDUpdateStrafeDirection(strafeDirection);
	}

	private void handleMovement(Vector2 movementVector) {
		bool _isMoving = movementVector.y != 0;
		bool _isStrafing = movementVector.x != 0;

		if (Mathf.Abs(movementVector.y) < Mathf.Abs(movementVector.x)) {
			if (_isMoving) _isMoving = false;
		}

		isWalking = _isMoving;
		isStrafing = _isStrafing;
		if (_isMoving) {
			if (movementVector.y > 0) {
				walkingSpeed = 1f; // front
			} else {
				walkingSpeed = -1f; // back
			}
		} else if (_isStrafing) {
			if (movementVector.x > 0) {
				strafeDirection = 1f; // right
			} else {
				strafeDirection = -1f; // left
			}
		}
	}

	private void updateStopAnimation(InputAction.CallbackContext obj) {
		if (headMove) {
			lastHeadPosition = cameraTransform.position;
		}

		isAnimatingLegs = false;

		isWalking = false;
		isStrafing = false;
		strafeDirection = 0f;
		walkingSpeed = 0f;

		CMDUpdateIsWalking(isWalking);
		CMDUpdateIsStrafing(isStrafing);
		CMDUpdateWalkingSpeed(walkingSpeed);
		CMDUpdateStrafeDirection(strafeDirection);
	}

	/// <summary>
	/// Setting Animator variables
	/// </summary>
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

			animator.SetFloat("animationSpeed", walkingSpeed);
		} else if (isStrafing) {
			animator.SetBool("isWalking", false);
			animator.SetFloat("strafeSpeed", 1);

			animator.SetBool("isStrafingRight", strafeDirection == 1f);
			animator.SetBool("isStrafingLeft", strafeDirection == -1f);
		}
	}

	/// <summary>
	/// Stoppings animations on animator
	/// </summary>
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
