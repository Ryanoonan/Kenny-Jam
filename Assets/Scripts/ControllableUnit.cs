using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AI;
using System.Linq;

public class ControllableUnit : MonoBehaviour
{
    public float moveSpeed = 5f;

    public float rotationSpeed = 15f; // Speed at which the unit rotates

    public GameManagerScript gameManagerScript; // Reference to the GameManagerScript
    public Vector3 startPosition;

    public enum ActionType
    {
        None,
        PickUp,
        Drop,
    }

    public List<Tuple<GameObject, ActionType>> targets = new List<Tuple<GameObject, ActionType>>();

    private FieldOfView fieldOfView;

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
        fieldOfView = GetComponent<FieldOfView>();
        if (patrolPoints.Count > 0)
        {
            targets.Add(Tuple.Create(patrolPoints[0].gameObject, ActionType.None));
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
            if (agent != null)
            {
                agent.enabled = true;
                Patrol();
            }
            if (fieldOfView != null)
            {
                fieldOfView.isActive = true; // Enable FOV if it exists
            }

        }
        else
        {
            if (agent != null)
            {
                agent.enabled = false;
            }
            if (fieldOfView != null)
            {
                fieldOfView.isActive = false; // Enable FOV if it exists
            }

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
        if (patrolPoints == null || patrolPoints.Count == 0 || targets.Count == 0) return;

        // Get the first target from the dictionary
        var firstTarget = GetFirstTarget();
        if (firstTarget.Item1 == null) return;

        GameObject currentTargetObj = firstTarget.Item1;
        ActionType currentAction = firstTarget.Item2;


        if (Vector3.Distance(transform.position, currentTargetObj.transform.position) < waypointThreshold)
        {
            // Perform the action based on the ActionType
            PerformAction(currentTargetObj, currentAction);

            // Remove the reached target
            targets.RemoveAt(0);
            if (currentAction == ActionType.PickUp)
            {
                Debug.Log("Picked up item: " + currentTargetObj.name);
            }

            // Check if this was a patrol point and add the next one
            Transform currentTransform = currentTargetObj.transform;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                if (patrolPoints[i] == currentTransform)
                {
                    int nextPatrolIndex = (i + 1) % patrolPoints.Count;
                    targets.Add(Tuple.Create(patrolPoints[nextPatrolIndex].gameObject, ActionType.None));
                    break;
                }
            }
        }

        // Move towards the first target in the dictionary if we still have targets
        var nextTarget = GetFirstTarget();
        if (nextTarget.Item1 != null)
        {
            agent.SetDestination(nextTarget.Item1.transform.position);
        }
    }

    private Tuple<GameObject, ActionType> GetFirstTarget()
    {
        foreach (var target in targets)
        {
            return target; // Return the first item in the dictionary
        }
        return new Tuple<GameObject, ActionType>(null, ActionType.None);
    }

    private void PerformAction(GameObject targetObj, ActionType action)
    {
        switch (action)
        {
            case ActionType.None:
                // Just reaching a waypoint, no special action needed
                break;
            case ActionType.PickUp:
                // Try to pick up the interactable item
                InteractableItem item = targetObj.GetComponent<InteractableItem>();
                if (item != null && currentItem == null)
                {
                    PickUpItem(item);
                }
                break;
            case ActionType.Drop:
                // Drop the current item at this location
                if (currentItem != null)
                {
                    DropItem();
                }
                Destroy(targetObj); // Remove the temporary GameObject
                break;
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

    public void ForceStop()
    {
        rb.linearVelocity = Vector3.zero;
    }


    public void PickUpItem(InteractableItem item)
    {

        item.transform.SetParent(transform);
        item.transform.localPosition = new Vector3(0, 1f, 0); // Adjust the Y value as needed
        currentItem = item; // Store the picked-up item

        if (item.CompareTag("Battery"))
        {
            moveSpeed *= 0.5f; // Reduce speed (adjust factor as needed)
        }
    }

    public void DropItem()
    {
        if (currentItem != null && currentItem.CompareTag("Battery"))
        {
            moveSpeed *= 2f; // Reset speed (inverse of multiplier used above)
        }
        currentItem.transform.SetParent(null);
        currentItem.transform.position = transform.position; // Drop above the unit
        gameManagerScript.ItemDropped(currentItem); // Notify the game manager
        currentItem = null; // Clear the current item reference

    }

    public void SetVelocity(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    /// <summary>
    /// Called when this unit's field of view detects an InteractableItem
    /// </summary>
    /// <param name="interactableObject">The GameObject containing the InteractableItem</param>
    public void FoundInteractibleObject(InteractableItem interactableItem)

    {
        // Add your logic here for when an interactable object is found
        if ((interactableItem.startPosition - interactableItem.transform.position).magnitude > 1f && !targets.Exists(t => t.Item1.name == $"StartPosition_{interactableItem.name}"))
        {
            // If the item has moved from its start position, we can consider it as a target
            Debug.Log("Found interactable item: " + interactableItem.name);
            // Create a temporary transform for the start position and add as Drop action
            GameObject startPosObj = new GameObject("StartPosition_" + interactableItem.name);
            startPosObj.transform.position = interactableItem.startPosition;
            // Add current position as PickUp action
            targets.Insert(0, Tuple.Create(interactableItem.gameObject, ActionType.PickUp));
            targets.Insert(1, Tuple.Create(startPosObj, ActionType.Drop));



        }
        // Example: You might want to move towards the object, highlight it, etc.
        // For now, just logging the found object
    }
}