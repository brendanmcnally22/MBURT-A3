using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNoiseEmitter : MonoBehaviour
{
    [Header("References")]
    public PlayerInput playerInput;
    public CharacterController controller;

    [Header("Noise Settings")]
    public float walkNoiseRadius = 4f;
    public float sprintNoiseRadius = 8f;
    public float landingNoiseRadius = 6f;

    public float walkLoudness = 1f;
    public float sprintLoudness = 1.6f;
    public float landingLoudness = 1.3f;

    public float walkNoiseInterval = 0.75f;
    public float sprintNoiseInterval = 0.35f;

    [Header("Optional")]
    public LayerMask catLayerMask = ~0;

    float noiseTimer;
    bool wasGrounded;

    void Awake()
    {
        if (!playerInput)
            playerInput = GetComponent<PlayerInput>();

        if (!controller)
            controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        wasGrounded = controller != null && controller.isGrounded;
    }

    void Update()
    {
        if (playerInput == null || controller == null)
            return;

        Vector2 moveInput = playerInput.currentActionMap["Move"].ReadValue<Vector2>();
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        bool isSprinting = false;

        if (playerInput.currentActionMap.FindAction("Sprint") != null)
            isSprinting = playerInput.currentActionMap["Sprint"].IsPressed();

        bool grounded = controller.isGrounded;

        // this gives a little landing sound event so the cat can react to heavier movement too.
        if (!wasGrounded && grounded)
            EmitNoise(transform.position, landingNoiseRadius, landingLoudness);

        wasGrounded = grounded;

        if (!grounded || !isMoving)
        {
            noiseTimer = 0f;
            return;
        }

        noiseTimer -= Time.deltaTime;

        float interval = isSprinting ? sprintNoiseInterval : walkNoiseInterval;

        if (noiseTimer <= 0f)
        {
            float radius = isSprinting ? sprintNoiseRadius : walkNoiseRadius;
            float loudness = isSprinting ? sprintLoudness : walkLoudness;

            EmitNoise(transform.position, radius, loudness);
            noiseTimer = interval;
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

        for (int i = 0; i < hits.Length; i++)
        {
            CatAI cat = hits[i].GetComponentInParent<CatAI>();

            if (cat != null)
                cat.ReportNoise(position, loudness);
        }
    }
}