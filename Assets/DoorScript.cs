using UnityEngine;

public class DoorScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private int blockViewLayer = 7; // Default layer for blocking view
    private GameObject viewBlocker;
    void Start()
    {
        viewBlocker = transform.Find("viewBlocker")?.gameObject;
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
        doorAnimator.SetBool("open", true);
        viewBlocker.layer = 0;
    }

    private void OnTriggerExit(Collider other)
    {
        doorAnimator.SetBool("open", false);
        viewBlocker.layer = blockViewLayer;
    }

}
