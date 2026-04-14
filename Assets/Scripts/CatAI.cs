using UnityEngine;
using UnityEngine.AI;

public class CatAI : MonoBehaviour
{
    // ================= BASE STATE =================
    public abstract class State
    {
        protected CatAI ai;
        public State(CatAI ai) { this.ai = ai; }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }

    // ================= STATES =================
    State currentState;

    HuntState huntState;
    MeowState meowState;
    SneakState sneakState;
    ChaseState chaseState;
    SearchState searchState;

    // ================= COMPONENTS =================
    public NavMeshAgent agent;
    public Transform player;
    public Transform[] huntPoints;
    public AudioSource audioSource;
    public AudioClip meowClip;

    // ================= SETTINGS =================
    public float huntSpeed = 3.5f;
    public float sneakSpeed = 2.5f;
    public float chaseSpeed = 6f;

    public float viewRadius = 10f;
    public float catchRange = 1.3f;

    public float meowTime = 1.5f;
    public float searchTime = 4f;

    public float memoryTime = 2f;

    // ================= MEMORY =================
    public Vector3 lastSeenPos;
    public float sightMemory;

    int huntIndex;

    // ================= UNITY =================
    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();

        huntState = new HuntState(this);
        meowState = new MeowState(this);
        sneakState = new SneakState(this);
        chaseState = new ChaseState(this);
        searchState = new SearchState(this);

        SwitchState(huntState);
    }

    void Update()
    {
        currentState?.Update();
    }

    public void SwitchState(State newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    // ================= HELPERS =================
    public bool CanSeePlayer()
    {
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= viewRadius;
    }

    public void MoveToNextPoint()
    {
        if (huntPoints.Length == 0) return;

        huntIndex = (huntIndex + 1) % huntPoints.Length;
        agent.SetDestination(huntPoints[huntIndex].position);
    }

    public void CatchPlayer()
    {
        Character c = player.GetComponent<Character>();
        if (c) c.Caught();

        if (GameManager.instance)
            GameManager.instance.ShowCaught();
    }

    // ================= STATES =================

    // -------- HUNT --------
    class HuntState : State
    {
        public HuntState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.speed = ai.huntSpeed;
            ai.MoveToNextPoint();
        }

        public override void Update()
        {
            if (ai.CanSeePlayer())
            {
                ai.SwitchState(ai.sneakState);
                return;
            }

            if (!ai.agent.pathPending && ai.agent.remainingDistance < 0.5f)
                ai.MoveToNextPoint();
        }
    }

    // -------- MEOW --------
    class MeowState : State
    {
        float t;

        public MeowState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            t = ai.meowTime;
            ai.agent.ResetPath();

            if (ai.audioSource && ai.meowClip)
                ai.audioSource.PlayOneShot(ai.meowClip);
        }

        public override void Update()
        {
            if (ai.CanSeePlayer())
            {
                ai.SwitchState(ai.sneakState);
                return;
            }

            t -= Time.deltaTime;
            if (t <= 0)
                ai.SwitchState(ai.huntState);
        }
    }

    // -------- SNEAK --------
    class SneakState : State
    {
        public SneakState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.speed = ai.sneakSpeed;
        }

        public override void Update()
        {
            if (ai.CanSeePlayer())
            {
                ai.lastSeenPos = ai.player.position;
                ai.sightMemory = ai.memoryTime;

                ai.agent.SetDestination(ai.player.position);

                float dist = Vector3.Distance(ai.transform.position, ai.player.position);
                if (dist <= ai.catchRange)
                    ai.SwitchState(ai.chaseState);
            }
            else
            {
                ai.sightMemory -= Time.deltaTime;

                if (ai.sightMemory <= 0)
                    ai.SwitchState(ai.searchState);
            }
        }
    }

    // -------- CHASE --------
    class ChaseState : State
    {
        public ChaseState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.speed = ai.chaseSpeed;
        }

        public override void Update()
        {
            if (ai.CanSeePlayer())
            {
                ai.lastSeenPos = ai.player.position;
                ai.sightMemory = ai.memoryTime;

                ai.agent.SetDestination(ai.player.position);

                float dist = Vector3.Distance(ai.transform.position, ai.player.position);

                if (dist <= ai.catchRange)
                {
                    ai.CatchPlayer();
                }
            }
            else
            {
                ai.sightMemory -= Time.deltaTime;

                if (ai.sightMemory <= 0)
                    ai.SwitchState(ai.searchState);
            }
        }
    }

    // -------- SEARCH --------
    class SearchState : State
    {
        float t;

        public SearchState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            t = ai.searchTime;
            ai.agent.speed = ai.huntSpeed;
            ai.agent.SetDestination(ai.lastSeenPos);
        }

        public override void Update()
        {
            if (ai.CanSeePlayer())
            {
                ai.SwitchState(ai.chaseState);
                return;
            }

            t -= Time.deltaTime;

            if (t <= 0)
                ai.SwitchState(ai.huntState);
        }
    }
}