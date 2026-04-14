using UnityEngine;
using UnityEngine.AI;

public class CatAI : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Search,
        Attack,
        Rest
    }

    public State currentState;

    public NavMeshAgent agent;
    public Transform[] patrolPoints;
    public Transform mouse;

    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6f;

    public float visionRange = 10f;
    public float attackRange = 1.6f;
    public float searchDuration = 4f;

    private int patrolIndex = 0;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ChangeState(State.Patrol);
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Search: Search(); break;
            case State.Attack: Attack(); break;
            case State.Rest: Rest(); break;
        }
    }

    void Patrol()
    {
        if (CanSeeMouse())
        {
            ChangeState(State.Chase);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            NextPatrolPoint();
    }

    void Chase()
    {
        if (mouse == null || !mouse.gameObject.activeSelf)
        {
            ChangeState(State.Patrol);
            return;
        }

        agent.SetDestination(mouse.position);

        float dist = Vector3.Distance(transform.position, mouse.position);

        if (dist <= attackRange)
        {
            ChangeState(State.Attack);
            return;
        }

        if (!CanSeeMouse())
        {
            timer = searchDuration;
            ChangeState(State.Search);
        }
    }

    void Search()
    {
        agent.SetDestination(mouse.position);

        if (CanSeeMouse())
        {
            ChangeState(State.Chase);
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
            ChangeState(State.Patrol);
    }

    void Attack()
    {
        if (mouse != null && mouse.gameObject.activeSelf)
        {
            float dist = Vector3.Distance(transform.position, mouse.position);

            if (dist <= attackRange)
            {
                Debug.Log("Cat caught mouse!");
                mouse.gameObject.SetActive(false);
            }
        }

        timer = 2f;
        ChangeState(State.Rest);
    }

    void Rest()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
            ChangeState(State.Patrol);
    }

    bool CanSeeMouse()
    {
        if (mouse == null || !mouse.gameObject.activeSelf) return false;

        return Vector3.Distance(transform.position, mouse.position) <= visionRange;
    }

    void NextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[patrolIndex].position);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    void ChangeState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Patrol:
                agent.speed = patrolSpeed;
                NextPatrolPoint();
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                break;

            case State.Rest:
                agent.ResetPath();
                break;
        }
    }
}