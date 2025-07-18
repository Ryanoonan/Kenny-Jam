using UnityEngine;
using System.Collections.Generic;

public class ControllableUnit : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;

    [Header("Patrol Settings")]
    public List<Transform> patrolPoints;        // Assigned in Inspector
    public float waypointThreshold = 0.2f;

    private int currentPatrolIndex = 0;
    private bool isControlled = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
    }

    void FixedUpdate()
    {
        // Patrol only if not being controlled
        if (!isControlled)
        {
            Patrol();
        }
    }

    public void Move(Vector2 input)
    {
        if (input.magnitude > 1f)
        {
            input.Normalize();
        }

        Vector3 inputDir = new Vector3(input.x, 0, input.y);
        Vector3 targetPosition = transform.position + inputDir * moveSpeed * Time.deltaTime;

        rb.MovePosition(targetPosition);
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;
    }

    public void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        Transform target = patrolPoints[currentPatrolIndex];
        Vector3 direction = (target.position - transform.position);
        direction.y = 0; // Keep movement horizontal only

        if (direction.magnitude < waypointThreshold)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        }
        else
        {
            Vector3 moveDir = direction.normalized;
            rb.MovePosition(transform.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Count < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            Vector3 from = patrolPoints[i].position;
            Vector3 to = patrolPoints[(i + 1) % patrolPoints.Count].position;
            Gizmos.DrawLine(from, to);
            Gizmos.DrawSphere(from, 0.2f);
        }
    }
}
