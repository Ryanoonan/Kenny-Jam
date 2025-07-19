using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AI;

public class ControllableUnit : MonoBehaviour
{
    public float moveSpeed = 5f;

    public float rotationSpeed = 15f; // Speed at which the unit rotates

    public GameManagerScript gameManagerScript; // Reference to the GameManagerScript
    public Vector3 startPosition;

    public Transform target; // The current target for the unit to move towards if not being controlled

    private Rigidbody rb;

    [Header("Patrol Settings")]
    public List<Transform> patrolPoints;        // Assigned in Inspector
    public float waypointThreshold = 0.2f;

    private int currentPatrolIndex = 0;
    private bool isControlled = false;

    public Animator animator; // Assign this in Inspector

    public float speed = 0;

    Vector3 lastPosition = Vector3.zero;

    public InteractableItem currentItem; // The item currently held by the unit
    NavMeshAgent agent;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (patrolPoints.Count > 0)
        {
            target = patrolPoints[0];
        }
    }
    void FixedUpdate()
    {
        speed = (transform.position - lastPosition).magnitude;
        Vector3 positionDiff = transform.position - lastPosition;
        lastPosition = transform.position;
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
        Debug.Log("Patrolling to point: " + currentPatrolIndex);
        if (Vector3.Distance(transform.position, target.position) < waypointThreshold)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count; // Loop through patrol points
            target = patrolPoints[currentPatrolIndex];
        }
        agent.SetDestination(target.position);
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

    public void ForceStop()
    {
        rb.linearVelocity = Vector3.zero;
    }


    public void PickUpItem(InteractableItem item)
    {

        item.transform.SetParent(transform);
        item.transform.localPosition = new Vector3(0, 1f, 0); // Adjust the Y value as needed
        currentItem = item; // Store the picked-up item
    }

    public void DropItem()
    {
        currentItem.transform.SetParent(null);
        currentItem.transform.position = transform.position; // Drop above the unit
        gameManagerScript.ItemDropped(currentItem); // Notify the game manager

    }

    public void SetVelocity(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }
}