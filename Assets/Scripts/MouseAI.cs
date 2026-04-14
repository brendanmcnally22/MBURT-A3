using UnityEngine;
using UnityEngine.AI;

public class MouseAI : MonoBehaviour
{
    public enum State
    {
        Gather,
        Flee,
        ReturnHome
    }

    public State currentState;

    public NavMeshAgent agent;
    public Transform cat;
    public Transform home;

    public float normalSpeed = 4f;
    public float fleeSpeed = 6f;
    public float dangerRange = 7f;

    private Cheese targetCheese;
    private bool carryingCheese = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ChangeState(State.Gather);
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Gather: Gather(); break;
            case State.Flee: Flee(); break;
            case State.ReturnHome: ReturnHome(); break;
        }
    }

    void Gather()
    {
        if (CatNear())
        {
            ChangeState(State.Flee);
            return;
        }

        if (targetCheese == null)
            targetCheese = FindClosestCheese();

        if (targetCheese == null) return;

        agent.SetDestination(targetCheese.transform.position);

        if (Vector3.Distance(transform.position, targetCheese.transform.position) < 1.3f)
        {
            targetCheese.Collect();
            targetCheese = null;
            carryingCheese = true;
            ChangeState(State.ReturnHome);
        }
    }

    void Flee()
    {
        Vector3 dir = (transform.position - cat.position).normalized;
        Vector3 fleeTarget = transform.position + dir * 8f;

        agent.SetDestination(fleeTarget);

        if (!CatNear())
        {
            ChangeState(carryingCheese ? State.ReturnHome : State.Gather);
        }
    }

    void ReturnHome()
    {
        if (CatNear())
        {
            ChangeState(State.Flee);
            return;
        }

        agent.SetDestination(home.position);

        if (Vector3.Distance(transform.position, home.position) < 1.2f)
        {
            carryingCheese = false;
            GameManager.instance.AddScore(1);
            ChangeState(State.Gather);
        }
    }

    bool CatNear()
    {
        if (cat == null || !cat.gameObject.activeSelf) return false;

        return Vector3.Distance(transform.position, cat.position) <= dangerRange;
    }

    Cheese FindClosestCheese()
    {
        Cheese[] cheeses = FindObjectsOfType<Cheese>();

        float best = Mathf.Infinity;
        Cheese closest = null;

        foreach (Cheese c in cheeses)
        {
            if (!c.gameObject.activeSelf) continue;

            float d = Vector3.Distance(transform.position, c.transform.position);

            if (d < best)
            {
                best = d;
                closest = c;
            }
        }

        return closest;
    }

    void ChangeState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Gather:
            case State.ReturnHome:
                agent.speed = normalSpeed;
                break;

            case State.Flee:
                agent.speed = fleeSpeed;
                break;
        }
    }
}