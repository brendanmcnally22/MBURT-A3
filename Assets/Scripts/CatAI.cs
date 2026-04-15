using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class CatAI : MonoBehaviour
{
    public enum MeowMood
    {
        Happy,
        Alert,
        Frustrated
    }

    public abstract class State
    {
        protected CatAI ai;

        public State(CatAI ai)
        {
            this.ai = ai;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }

    State currentState;

    RouteState routeState;
    MeowState meowState;
    SneakState sneakState;
    ChaseState chaseState;
    SearchState searchState;
    InvestigateState investigateState;
    HomeWatchState homeWatchState;

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Transform[] routePoints;
    public Transform homePoint;
    public AudioSource audioSource;
    public TextMeshProUGUI stateText;

    [Header("Audio")]
    public AudioClip happyMeowClip;
    public AudioClip alertMeowClip;
    public AudioClip frustratedMeowClip;

    [Header("Movement")]
    public float routeSpeed = 2.5f;
    public float sneakSpeed = 2f;
    public float chaseSpeed = 5.5f;
    public float homeWatchSpeed = 2.2f;

    [Header("Vision")]
    public float viewRadius = 10f;
    [Range(1f, 360f)] public float viewAngle = 110f;
    public LayerMask visionBlockers = ~0;

    [Header("Ranges")]
    public float chaseRange = 2.8f;
    public float catchRange = 1.3f;

    [Header("Memory")]
    public float sightMemoryTime = 1.25f;
    public float searchTime = 4f;
    public float hearingMemoryTime = 3f;
    public float hearingRadius = 12f;

    [Header("Route Behaviour")]
    public float waitAtPointTime = 1f;

    [Header("Meow Behaviour")]
    public float meowDuration = 1.2f;
    public float randomRouteMeowChance = 0.25f;
    public float routeMeowCooldown = 5f;

    [Header("Sneak Behaviour")]
    public float minimumSneakTime = 1.2f;
    public float sneakRepathTime = 0.45f;
    public float sneakSideMin = 1.4f;
    public float sneakSideMax = 2.8f;
    public float sneakBackOffset = 1.5f;

    [Header("Search Escalation")]
    public int failedSearchesBeforeHomeWatch = 2;
    public float homeWatchTime = 5f;
    public float homeCircleRadius = 2f;
    public int homeCircleSteps = 3;

    int routeIndex;
    int failedSearchCount;

    Vector3 lastSeenPosition;
    Vector3 lastHeardPosition;

    float sightMemoryTimer;
    float hearingMemoryTimer;
    float meowCooldownTimer;

    MeowMood pendingMood = MeowMood.Happy;

    void Start()
    {
        if (!agent)
            agent = GetComponent<NavMeshAgent>();

        routeState = new RouteState(this);
        meowState = new MeowState(this);
        sneakState = new SneakState(this);
        chaseState = new ChaseState(this);
        searchState = new SearchState(this);
        investigateState = new InvestigateState(this);
        homeWatchState = new HomeWatchState(this);

        SwitchState(routeState);
    }

    void Update()
    {
        if (sightMemoryTimer > 0f)
            sightMemoryTimer -= Time.deltaTime;

        if (hearingMemoryTimer > 0f)
            hearingMemoryTimer -= Time.deltaTime;

        if (meowCooldownTimer > 0f)
            meowCooldownTimer -= Time.deltaTime;

        currentState?.Update();
    }

    public void SwitchState(State newState)
    {
        if (newState == null)
            return;

        if (currentState == newState)
            return;

        currentState?.Exit();
        currentState = newState;
        currentState.Enter();

        if (stateText)
            stateText.text = currentState.GetType().Name.Replace("State", "");
    }

    public bool CanSeePlayer()
    {
        if (!player)
            return false;

        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Vector3 target = player.position + Vector3.up * 0.5f;

        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (dist > viewRadius)
            return false;

        if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f)
            return false;

        if (Physics.Raycast(origin, dir.normalized, dist, visionBlockers))
            return false;

        lastSeenPosition = player.position;
        sightMemoryTimer = sightMemoryTime;
        failedSearchCount = 0;

        return true;
    }

    public void ReportNoise(Vector3 pos, float loudness)
    {
        float effectiveRadius = hearingRadius * Mathf.Clamp(loudness, 0.1f, 2f);

        if (Vector3.Distance(transform.position, pos) > effectiveRadius)
            return;

        lastHeardPosition = pos;
        hearingMemoryTimer = hearingMemoryTime;

        if (currentState != chaseState && currentState != sneakState)
        {
            pendingMood = MeowMood.Alert;
            SwitchState(investigateState);
        }
    }

    public void PlayMeow(MeowMood mood)
    {
        if (!audioSource)
            return;

        switch (mood)
        {
            case MeowMood.Alert:
                if (alertMeowClip) audioSource.PlayOneShot(alertMeowClip);
                break;

            case MeowMood.Frustrated:
                if (frustratedMeowClip) audioSource.PlayOneShot(frustratedMeowClip);
                break;

            default:
                if (happyMeowClip) audioSource.PlayOneShot(happyMeowClip);
                break;
        }
    }

    public void CatchPlayer()
    {
        Character c = player ? player.GetComponent<Character>() : null;
        if (c) c.Caught();

        if (GameManager.instance)
            GameManager.instance.ShowCaught();
    }

    public bool PlayerIsHome()
    {
        Character c = player ? player.GetComponent<Character>() : null;
        return c != null && c.inHome;
    }

    public void RegisterFailedSearch()
    {
        failedSearchCount++;
    }

    public bool ShouldEscalateToHomeWatch()
    {
        return homePoint && PlayerIsHome() && failedSearchCount >= failedSearchesBeforeHomeWatch;
    }

    public void MoveToNextRoutePoint()
    {
        if (routePoints == null || routePoints.Length == 0)
            return;

        routeIndex %= routePoints.Length;
        SetDestinationSafe(routePoints[routeIndex].position);
        routeIndex = (routeIndex + 1) % routePoints.Length;
    }

    public void SetDestinationSafe(Vector3 target)
    {
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(target);
    }

    public Vector3 RandomPointNear(Vector3 center, float radius)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 random = center + Random.insideUnitSphere * radius;
            random.y = center.y;

            if (NavMesh.SamplePosition(random, out NavMeshHit hit, radius, NavMesh.AllAreas))
                return hit.position;
        }

        return center;
    }

    public bool ReachedDestination()
    {
        if (agent.pathPending)
            return false;

        if (!agent.hasPath)
            return true;

        return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.5f);
    }

    public void FaceMoveDirection(float turnSpeed = 8f)
    {
        Vector3 velocity = agent.desiredVelocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    class RouteState : State
    {
        float waitTimer;
        bool waiting;

        public RouteState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.speed = ai.routeSpeed;
            waiting = false;
            ai.MoveToNextRoutePoint();
        }

        public override void Update()
        {
            ai.FaceMoveDirection();

            if (ai.CanSeePlayer())
            {
                ai.pendingMood = MeowMood.Alert;
                ai.SwitchState(ai.sneakState);
                return;
            }

            if (ai.hearingMemoryTimer > 0f)
            {
                ai.SwitchState(ai.investigateState);
                return;
            }

            if (!waiting && ai.ReachedDestination())
            {
                waiting = true;
                waitTimer = ai.waitAtPointTime;

                if (ai.meowCooldownTimer <= 0f && Random.value < ai.randomRouteMeowChance)
                {
                    ai.pendingMood = MeowMood.Happy;
                    ai.SwitchState(ai.meowState);
                    return;
                }
            }

            if (waiting)
            {
                waitTimer -= Time.deltaTime;

                if (waitTimer <= 0f)
                {
                    waiting = false;
                    ai.MoveToNextRoutePoint();
                }
            }
        }
    }

    class SneakState : State
    {
        float repathTimer;
        float sideDirection;
        float sneakTimer;

        public SneakState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.speed = ai.sneakSpeed;
            repathTimer = 0f;
            sideDirection = Random.value < 0.5f ? -1f : 1f;
            sneakTimer = ai.minimumSneakTime;
        }

        public override void Update()
        {
            ai.FaceMoveDirection(10f);

            if (!ai.player)
            {
                ai.SwitchState(ai.routeState);
                return;
            }

            bool sees = ai.CanSeePlayer();

            if (sees)
            {
                float dist = Vector3.Distance(ai.transform.position, ai.player.position);

                repathTimer -= Time.deltaTime;
                sneakTimer -= Time.deltaTime;

                if (repathTimer <= 0f)
                {
                    Vector3 toPlayer = (ai.player.position - ai.transform.position).normalized;
                    Vector3 side = Vector3.Cross(Vector3.up, toPlayer) * sideDirection;

                    Vector3 target =
                        ai.player.position +
                        side * Mathf.Clamp(dist * 0.45f, ai.sneakSideMin, ai.sneakSideMax) -
                        toPlayer * ai.sneakBackOffset;

                    ai.SetDestinationSafe(target);
                    repathTimer = ai.sneakRepathTime;
                }

                if (sneakTimer <= 0f && dist <= ai.chaseRange)
                {
                    ai.SwitchState(ai.chaseState);
                    return;
                }
            }
            else
            {
                if (ai.sightMemoryTimer > 0f)
                {
                    ai.SetDestinationSafe(ai.lastSeenPosition);
                }
                else
                {
                    ai.SwitchState(ai.searchState);
                }
            }
        }
    }

    class ChaseState : State
    {
        float loseTimer;

        public ChaseState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.speed = ai.chaseSpeed;
            loseTimer = ai.sightMemoryTime;
            ai.PlayMeow(MeowMood.Alert);
        }

        public override void Update()
        {
            ai.FaceMoveDirection(14f);

            bool sees = ai.CanSeePlayer();

            if (sees)
            {
                loseTimer = ai.sightMemoryTime;
                ai.SetDestinationSafe(ai.player.position);
            }
            else
            {
                loseTimer -= Time.deltaTime;
                ai.SetDestinationSafe(ai.lastSeenPosition);
            }

            float dist = Vector3.Distance(ai.transform.position, ai.player.position);

            if (dist <= ai.catchRange)
            {
                ai.CatchPlayer();
                return;
            }

            if (loseTimer <= 0f)
            {
                ai.pendingMood = MeowMood.Frustrated;
                ai.SwitchState(ai.searchState);
            }
        }
    }

    class SearchState : State
    {
        float timer;
        int step;
        Vector3 center;

        public SearchState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.speed = ai.routeSpeed;
            timer = ai.searchTime;
            step = 0;
            center = ai.lastSeenPosition;
            ai.SetDestinationSafe(center);
        }

        public override void Update()
        {
            ai.FaceMoveDirection();

            if (ai.CanSeePlayer())
            {
                ai.SwitchState(ai.chaseState);
                return;
            }

            if (ai.hearingMemoryTimer > 0f)
            {
                ai.SwitchState(ai.investigateState);
                return;
            }

            timer -= Time.deltaTime;

            if (ai.ReachedDestination())
            {
                step++;

                if (step >= 4)
                {
                    ai.RegisterFailedSearch();

                    if (ai.ShouldEscalateToHomeWatch())
                    {
                        ai.pendingMood = MeowMood.Alert;
                        ai.SwitchState(ai.homeWatchState);
                    }
                    else
                    {
                        ai.pendingMood = MeowMood.Frustrated;
                        ai.SwitchState(ai.meowState);
                    }

                    return;
                }

                ai.SetDestinationSafe(ai.RandomPointNear(center, 3f));
            }

            if (timer <= 0f)
            {
                ai.RegisterFailedSearch();

                if (ai.ShouldEscalateToHomeWatch())
                {
                    ai.pendingMood = MeowMood.Alert;
                    ai.SwitchState(ai.homeWatchState);
                }
                else
                {
                    ai.pendingMood = MeowMood.Frustrated;
                    ai.SwitchState(ai.meowState);
                }
            }
        }
    }

    class InvestigateState : State
    {
        float timer;
        int step;
        Vector3 center;

        public InvestigateState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.speed = ai.routeSpeed;
            timer = ai.hearingMemoryTime;
            step = 0;
            center = ai.lastHeardPosition;
            ai.SetDestinationSafe(center);
        }

        public override void Update()
        {
            ai.FaceMoveDirection();

            if (ai.CanSeePlayer())
            {
                ai.SwitchState(ai.chaseState);
                return;
            }

            timer -= Time.deltaTime;

            if (ai.ReachedDestination())
            {
                step++;

                if (step >= 3)
                {
                    ai.RegisterFailedSearch();

                    if (ai.ShouldEscalateToHomeWatch())
                    {
                        ai.pendingMood = MeowMood.Alert;
                        ai.SwitchState(ai.homeWatchState);
                    }
                    else
                    {
                        ai.pendingMood = MeowMood.Frustrated;
                        ai.SwitchState(ai.meowState);
                    }

                    return;
                }

                ai.SetDestinationSafe(ai.RandomPointNear(center, 2.5f));
            }

            if (timer <= 0f)
            {
                ai.RegisterFailedSearch();

                if (ai.ShouldEscalateToHomeWatch())
                {
                    ai.pendingMood = MeowMood.Alert;
                    ai.SwitchState(ai.homeWatchState);
                }
                else
                {
                    ai.pendingMood = MeowMood.Frustrated;
                    ai.SwitchState(ai.meowState);
                }
            }
        }
    }

    class HomeWatchState : State
    {
        float timer;
        int circleStep;
        Vector3 center;

        public HomeWatchState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.speed = ai.homeWatchSpeed;
            timer = ai.homeWatchTime;
            circleStep = 0;
            center = ai.homePoint ? ai.homePoint.position : ai.lastSeenPosition;

            ai.SetDestinationSafe(center);
            ai.PlayMeow(MeowMood.Alert);
        }

        public override void Update()
        {
            ai.FaceMoveDirection();

            if (ai.CanSeePlayer())
            {
                ai.failedSearchCount = 0;
                ai.SwitchState(ai.chaseState);
                return;
            }

            timer -= Time.deltaTime;

            if (ai.ReachedDestination())
            {
                circleStep++;

                if (circleStep >= ai.homeCircleSteps)
                {
                    ai.pendingMood = MeowMood.Frustrated;
                    ai.SwitchState(ai.meowState);
                    return;
                }

                Vector3 offset =
                    Quaternion.Euler(0f, circleStep * (360f / Mathf.Max(1, ai.homeCircleSteps)), 0f) *
                    Vector3.forward *
                    ai.homeCircleRadius;

                ai.SetDestinationSafe(center + offset);
            }

            if (timer <= 0f)
            {
                ai.pendingMood = MeowMood.Frustrated;
                ai.SwitchState(ai.meowState);
            }
        }
    }

    class MeowState : State
    {
        float timer;

        public MeowState(CatAI ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.ResetPath();
            timer = ai.meowDuration;
            ai.PlayMeow(ai.pendingMood);
            ai.meowCooldownTimer = ai.routeMeowCooldown;
        }

        public override void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                ai.pendingMood = MeowMood.Happy;
                ai.SwitchState(ai.routeState);
            }
        }
    }
}