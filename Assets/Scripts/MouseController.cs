using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Optional Sprint")]
    public float sprintSpeed = 8f;

    CharacterController controller;
    PlayerInput playerInput;

    Vector3 velocity;
    bool isCaught = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        if (isCaught) return;

        PlayerMotion();
    }

    void PlayerMotion()
    {
        // grounded reset
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // input
        Vector2 moveInput =
            playerInput.currentActionMap["Move"].ReadValue<Vector2>();

        Vector3 move =
            Vector3.right * moveInput.x +
            Vector3.forward * moveInput.y;

        // sprint check (optional if action exists)
        float speed = moveSpeed;

        if (playerInput.currentActionMap.FindAction("Sprint") != null)
        {
            if (playerInput.currentActionMap["Sprint"].IsPressed())
                speed = sprintSpeed;
        }

        Vector3 moveVelocity = move * speed;

        // gravity
        velocity.y += gravity * Time.deltaTime;
        moveVelocity.y = velocity.y;

        controller.Move(moveVelocity * Time.deltaTime);

        // rotate toward move direction
        Vector3 flatVelocity =
            new Vector3(moveVelocity.x, 0f, moveVelocity.z);

        if (flatVelocity.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(flatVelocity);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                15f * Time.deltaTime);
        }
    }

    public void Caught()
    {
        isCaught = true;
        enabled = false;
    }
}