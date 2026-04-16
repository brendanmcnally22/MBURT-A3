using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Optional Sprint")]
    public float sprintSpeed = 8f;

    public bool inHome;

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
        if (isCaught)
            return;

        PlayerMotion();
    }

    void PlayerMotion()
    {
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // grabbing input straight from the action map so i can keep movement stuff in one place.
        Vector2 moveInput = playerInput.currentActionMap["Move"].ReadValue<Vector2>();

        Vector3 move =
            Vector3.right * moveInput.x +
            Vector3.forward * moveInput.y;

        float speed = moveSpeed;

        // sprint is optional so i check if the action even exists first.
        if (playerInput.currentActionMap.FindAction("Sprint") != null)
        {
            if (playerInput.currentActionMap["Sprint"].IsPressed())
                speed = sprintSpeed;
        }

        Vector3 moveVelocity = move * speed;

        velocity.y += gravity * Time.deltaTime;
        moveVelocity.y = velocity.y;

        controller.Move(moveVelocity * Time.deltaTime);

        // this is just to make the player face where they are actually moving.
        Vector3 flatVelocity = new Vector3(moveVelocity.x, 0f, moveVelocity.z);

        if (flatVelocity.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatVelocity);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                15f * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // home is important because the cat uses this to decide
        // "ok you're probably hiding in there now."
        if (other.CompareTag("Home"))
            inHome = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Home"))
            inHome = false;
    }

    public void Caught()
    {
        // i disable the character after getting caught so the player doesn't keep moving during game over.
        isCaught = true;
        enabled = false;
    }
}