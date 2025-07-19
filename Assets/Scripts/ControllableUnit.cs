using UnityEngine;
using System.Collections.Generic;

public class ControllableUnit : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;
    public Vector3 startPosition;

    private Rigidbody rb;

    [Header("Patrol Settings")]
    public List<Transform> patrolPoints;
    public float waypointThreshold = 0.2f;
    private int currentPatrolIndex = 0;
    private bool isControlled = false;

    public Animator animator;
    private Vector3 lastPosition = Vector3.zero;

    private Vector3 inputDirection = Vector3.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (isControlled)
        {
            if (inputDirection.sqrMagnitude > 0.01f)
            {
                Vector3 attemptedMove = inputDirection * moveSpeed;
                Vector3 velocityToApply = attemptedMove;

                if (rb.SweepTest(inputDirection.normalized, out RaycastHit hit, moveSpeed * Time.fixedDeltaTime + 0.05f))
                {
                    // Try to slide along the surface
                    Vector3 slideDirection = Vector3.ProjectOnPlane(attemptedMove, hit.normal);

                    // Prevent getting stuck on flat walls by checking if projected move is big enough
                    if (slideDirection.sqrMagnitude > 0.01f)
                    {
                        velocityToApply = slideDirection;
                    }
                    else
                    {
                        velocityToApply = Vector3.zero; // fully blocked
                    }
                }

                rb.linearVelocity = velocityToApply;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }
        }

        else
        {
            // Patrol movement
            Patrol();
        }

        HandleRotation();

        // Animation handling
        float speed = rb.linearVelocity.magnitude;
        animator.SetBool("isMoving", speed > 0.05f);

        inputDirection = Vector3.zero;
    }

    public void Move(Vector2 input)
    {
        if (input.magnitude > 1f)
            input.Normalize();

        inputDirection = new Vector3(input.x, 0, input.y);
    }

    public void SetControlled(bool controlled)
    {
        isControlled = controlled;

        if (!controlled)
        {
            inputDirection = Vector3.zero; // clear input when not controlled
        }
    }

    public void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
            return;

        Transform target = patrolPoints[currentPatrolIndex];
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;

        if (direction.magnitude < waypointThreshold)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            Vector3 moveDir = direction.normalized;
            rb.linearVelocity = moveDir * moveSpeed;
        }
    }

    private void HandleRotation()
    {
        Vector3 linearVelocity = rb.linearVelocity;

        if (linearVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(linearVelocity.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
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
