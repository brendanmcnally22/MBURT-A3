using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNoiseEmitter : MonoBehaviour
{
    [Header("References")]
    public PlayerInput playerInput;
    public CharacterController controller;

    [Header("Noise Settings")]
    public float walkNoiseRadius = 5f;

    public float walkLoudness = 1f;

    public float walkNoiseInterval = 0.6f;

    [Header("Optional")]
    public LayerMask catLayerMask = ~0;

    float noiseTimer;

    void Awake()
    {
        if (!playerInput)
            playerInput = GetComponent<PlayerInput>();

        if (!controller)
            controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (playerInput == null || controller == null)
            return;

        Vector2 moveInput = playerInput.currentActionMap["Move"].ReadValue<Vector2>();
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        // originally i had landing noise detection here because i planned to support jumping
        // but later i simplified the player movement to stay grounded only
        // so i removed the landing logic to keep the noise system focused and intentional
        // instead of leaving behaviour in the script that the player could never trigger

        // if the player is not grounded or not moving i dont emit noise
        // this keeps the investigation behaviour readable and prevents constant noise spam
        if (!controller.isGrounded || !isMoving)
        {
            noiseTimer = 0f;
            return;
        }

        noiseTimer -= Time.deltaTime;

        // footsteps are emitted in timed intervals instead of every frame
        // i did this so the cat reacts to discrete sound events instead of tracking the player perfectly
        // which makes the stealth interaction feel more fair and understandable
        if (noiseTimer <= 0f)
        {
            EmitNoise(transform.position, walkNoiseRadius, walkLoudness);
            noiseTimer = walkNoiseInterval;
        }
    }

    void EmitNoise(Vector3 position, float radius, float loudness)
    {
        Collider[] hits = Physics.OverlapSphere(
            position,
            radius,
            catLayerMask,
            QueryTriggerInteraction.Ignore
        );

        // every cat inside the radius receives the sound event
        // i structured it this way so multiple cats could react independently if i expand the system later
        for (int i = 0; i < hits.Length; i++)
        {
            CatAI cat = hits[i].GetComponentInParent<CatAI>();

            // i check parent objects so the detection still works
            // even if the collider is not directly on the cat root object
            if (cat != null)
                cat.ReportNoise(position, loudness);
        }
    }
}