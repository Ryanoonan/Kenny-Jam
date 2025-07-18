using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerManagerScript : MonoBehaviour
{
    public GameObject spaceBar;              // Assign this in Inspector
    public Camera mainCamera;                // Assign this in Inspector
    public float switchDistance = 2f;        // How close you need to be to switch guards
    public KeyCode switchKey = KeyCode.Space;    // Key to press to switch guards

    public ControllableUnit selectedUnit;   // The currently controlled guard
    private List<ControllableUnit> allUnits = new List<ControllableUnit>();  // All guards in the scene

    void Start()
    {
        // Find all guards (units with ControllableUnit component)
        ControllableUnit[] units = FindObjectsByType<ControllableUnit>(FindObjectsSortMode.None);
        allUnits.AddRange(units);

        // Pick the first guard as the one we start controlling
        if (allUnits.Count > 0)
        {
            selectedUnit = allUnits[0];
        }


        CameraScript cameraScript = mainCamera.GetComponent<CameraScript>();
        cameraScript.SetTarget(selectedUnit.transform);

    }

    // Update runs every frame
    void Update()
    {
        HandleSwitch();
    }

    void FixedUpdate()
    {
        HandleMovement();   // Handle WASD/Arrow input
    }

    // Move the selected guard using input
    void HandleMovement()
    {
        if (selectedUnit == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0)
        {
            selectedUnit.Move(new Vector2(h, v));
        }
    }

    // Look for a nearby guard and switch to them on key press
    void HandleSwitch()
    {
        if (selectedUnit == null) return;
        ControllableUnit nearest = FindNearestSwitchableUnit();
        SpaceScript spaceScript = spaceBar.GetComponent<SpaceScript>();
        if (nearest != null)
        {
            spaceBar.SetActive(true);
            spaceScript.SetNearestUnit(nearest);

            // Check if the switch key is pressed first
            if (Input.GetKeyDown(switchKey))
            {
                Debug.Log("Switching guards...");

                // Update selected unit
                selectedUnit = nearest;
                Debug.Log("Switched to: " + nearest.name);

                // Update camera target
                CameraScript cameraScript = mainCamera.GetComponent<CameraScript>();
                cameraScript.SetTarget(selectedUnit.transform);

                // Call SetControlledUnit on the space bar game object
                GameObject spaceBar = GameObject.Find("Space Bar");

            }

        }
        else
        {
            spaceBar.SetActive(false);
        }







        // Finds the closest unit that is NOT the current one and is within range
        ControllableUnit FindNearestSwitchableUnit()
        {
            ControllableUnit nearest = null;
            float minDistance = Mathf.Infinity;

            foreach (var unit in allUnits)
            {
                if (unit == selectedUnit) continue;

                float distance = Vector3.Distance(selectedUnit.transform.position, unit.transform.position);
                if (distance <= switchDistance && distance < minDistance)
                {
                    minDistance = distance;
                    nearest = unit;
                }
            }

            return nearest;
        }


    }
}