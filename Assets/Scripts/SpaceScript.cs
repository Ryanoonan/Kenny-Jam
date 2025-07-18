using UnityEngine;
using UnityEngine.UIElements;

public class SpaceScript : MonoBehaviour
{
    private ControllableUnit nearestUnit; // The unit this script controls
    public Vector3 offset = new Vector3(0, 1, 0); // Offset from the unit position
    private SpriteRenderer spriteRenderer; // Reference to the sprite renderer
    private Color cachedColor;
    private bool wasUnitPresent;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        // Update position if unit is present
        if (nearestUnit)
        {
            transform.position = nearestUnit.transform.position + offset; // Position above the unit
        }
    }

    public void SetNearestUnit(ControllableUnit unit)
    {
        nearestUnit = unit;
    }

}
