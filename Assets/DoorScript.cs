using UnityEngine;

public class DoorScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Animator doorAnimator;

    void Awake()
    {
        // Get the animator component if it's not already assigned
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name);
        doorAnimator.SetBool("open", true);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger exited by: " + other.gameObject.name);
        doorAnimator.SetBool("open", false);
    }

}
