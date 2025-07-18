using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created]
    private Transform target; // The target to follow
    public GameObject playerManager;
    public Vector3 offset = new Vector3(0, 5, -10); // Offset from the target position
    void Start()
    {

    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = desiredPosition;

        // Optional: Keep the camera looking at the player
        transform.LookAt(target);
    }
}
