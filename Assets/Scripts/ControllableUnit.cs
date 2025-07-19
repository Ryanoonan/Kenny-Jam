using UnityEngine;
using System.Collections.Generic;
using System;

public class ControllableUnit : MonoBehaviour
{
    public float moveSpeed = 5f;

    public float rotationSpeed = 15f; // Speed at which the unit rotates

    public Vector3 startPosition;

    private Rigidbody rb;

    [Header("Patrol Settings")]
    public List<Transform> patrolPoints;        // Assigned in Inspector
    public float waypointThreshold = 0.2f;

    private int currentPatrolIndex = 0;
    private bool isControlled = false;

    public Animator animator; // Assign this in Inspector

    public float speed = 0;

    Vector3 lastPosition = Vector3.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    void Start()
    {
    }

    void FixedUpdate()
    {
        speed = (transform.position - lastPosition).magnitude;
        Vector3 positionDiff = transform.position - lastPosition;
        lastPosition = transform.position;
        Debug.Log(animator.GetBool("isMoving"));
        // Patrol only if not being controlled
        if (!isControlled)
        {
            Patrol();
        }
        if (speed > 0f)
        {
            animator.SetBool("isMoving", true); // Prevents Rigidbody from falling through the ground
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
        if (positionDiff.magnitude > 0.01f)
        {
            Vector3 direction = positionDiff.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime));
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

    public void PickUpItem(InteractableItem item)
    {
        item.transform.SetParent(transform);
        item.transform.localPosition = new Vector3(0, 1f, 0); // Adjust the Y value as needed
    }
}
