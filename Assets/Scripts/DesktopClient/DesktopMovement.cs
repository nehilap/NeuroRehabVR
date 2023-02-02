using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopMovement : MonoBehaviour {
    [SerializeField] private InputActionReference move;

    [SerializeField] AvatarWalkingController[] avatarWalkingControllers;

    [SerializeField] private CharacterController body;
    [SerializeField] private float moveSpeed = 7f;

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