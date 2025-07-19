using UnityEngine;

public class test2 : MonoBehaviour
{

    public Rigidbody body;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        body.MovePosition(new Vector3(0, 0, 0));
        Debug.Log("move");
    }
}
