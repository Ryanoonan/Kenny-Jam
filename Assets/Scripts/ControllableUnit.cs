using UnityEditor.Callbacks;
using UnityEngine;

public class ControllableUnit : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody rb; // Reference to the Rigidbody component
                          // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void Move(Vector2 input)
    {
        if (input.magnitude > 1f)
        {
            input.Normalize();
        }
        Vector3 inputDir = new Vector3(input.x, 0, input.y);
        rb.MovePosition(transform.position + inputDir * moveSpeed * Time.deltaTime);
    }
}
