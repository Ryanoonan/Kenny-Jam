using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector3 startPosition;

    void Awake()
    {
        startPosition = transform.position;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

}
