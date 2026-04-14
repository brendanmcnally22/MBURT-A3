using UnityEngine;
using UnityEngine.AI;

public class CatAI : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Search
    }

    public State currentState;

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Transform[] patrolPoints;

    [Header("Movement")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6f;

    [Header("Detection")]
    public float visionRange = 10f;
    public float catchRange = 1.5f;

    [Header("Search")]
    public float searchDuration = 4f;

    int patrolIndex = 0;
    float timer;
    Vector3 lastSeenPos;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        ChangeState(State.Patrol);
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Search:
                Search();
                break;
        }
    }

    void Patrol()
    {
        if (CanSeePlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void Chase()
    {
        agent.SetDestination(player.position);
        lastSeenPos = player.position;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= catchRange)
        {
            GameManager.instance.ShowCaught();
            enabled = false;
            return;
        }

        if (!CanSeePlayer())
        {
            timer = searchDuration;
            ChangeState(State.Search);
        }
    }

    void Search()
    {
        agent.SetDestination(lastSeenPos);

        if (CanSeePlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
            ChangeState(State.Patrol);
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        return Vector3.Distance(transform.position, player.position) <= visionRange;
    }

    void ChangeState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Patrol:
                agent.speed = patrolSpeed;
                if (patrolPoints.Length > 0)
                    agent.SetDestination(patrolPoints[patrolIndex].position);
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                break;
        }
    }
}