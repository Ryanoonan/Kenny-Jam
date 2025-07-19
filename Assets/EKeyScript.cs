using UnityEngine;
using UnityEngine.UIElements;

public class EKeyScript : MonoBehaviour
{
    private InteractableItem nearestItem; // The unit this script controls
    public Vector3 offset = new Vector3(0, 1, 0); // Offset from the unit position

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        // Update position if unit is present
        if (nearestItem)
        {
            transform.position = nearestItem.transform.position + offset; // Position above the unit
        }
    }

    public void SetNearestItem(InteractableItem unit)
    {
        nearestItem = unit;
    }

}
