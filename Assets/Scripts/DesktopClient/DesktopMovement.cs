using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopMovement : MonoBehaviour {
	[SerializeField] private InputActionReference move;

	[SerializeField] AvatarController[] avatarControllers;

	[SerializeField] private CharacterController body;
	[SerializeField] private float moveSpeed = 7f;

	void Start() {
		for (int i = 0; i < avatarControllers.Length; i++) {
			if (avatarControllers[i].gameObject.activeInHierarchy) {
				body.height *= avatarControllers[i].sizeMultiplier;
				body.center *= avatarControllers[i].sizeMultiplier;
			}
		}
	}

	void Update() {
		Vector2 tempMove = move.action.ReadValue<Vector2>();

		Vector3 sidewayMovement = transform.right *  tempMove.x;
		Vector3 forwardMovement = transform.forward *  tempMove.y;
		Vector3 movement =  sidewayMovement + forwardMovement;

		movement = Vector3.ClampMagnitude(movement, 1f); // double move speed fix

		Vector3 movementVelocity = movement * moveSpeed * Time.deltaTime;
		body.Move(movementVelocity);
	}
}