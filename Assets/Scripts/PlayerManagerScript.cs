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

    private float holdStartTime = 0f;
    private ControllableUnit pendingSwitchUnit = null;
    private bool isHoldingSwitch = false;
    private float maxHoldDuration = 3f;

    public void GameLoopStart()
    {
        // Find all controllable units in the scene
        ControllableUnit[] units = FindObjectsByType<ControllableUnit>(FindObjectsSortMode.None);
        allUnits.AddRange(units);

        // Specifically find the "Prisoner" unit and control it
        GameObject prisonerObj = GameObject.Find("Prisoner");

        if (prisonerObj != null)
        {
            ControllableUnit prisonerUnit = prisonerObj.GetComponent<ControllableUnit>();

            if (prisonerUnit != null)
            {
                selectedUnit = prisonerUnit;

                // Set camera to follow the prisoner
                CameraScript cameraScript = mainCamera.GetComponent<CameraScript>();
                cameraScript.SetTarget(selectedUnit.transform);
            }
            else
            {
                Debug.LogWarning("Prisoner does not have a ControllableUnit component.");
            }
        }
        else
        {
            Debug.LogWarning("GameObject named 'Prisoner' not found.");
        }
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

    void HandleSwitch()
    {
        SpaceScript spaceScript = spaceBar.GetComponent<SpaceScript>();

        // If not holding the switch key, check if we are near any switchable unit to enable UI and start hold
        if (!isHoldingSwitch)
        {
            ControllableUnit nearest = FindNearestSwitchableUnit();

            if (nearest != null)
            {
                spaceBar.SetActive(true);
                spaceScript.SetNearestUnit(nearest);

                // Start holding switch key
                if (Input.GetKeyDown(switchKey))
                {
                    isHoldingSwitch = true;
                    holdStartTime = Time.time;
                    pendingSwitchUnit = nearest;
                    Debug.Log($"Started holding switch key towards {nearest.name}");
                }
            }
            else
            {
                spaceBar.SetActive(false);
            }
        }
        else
        {
            // While holding the key
            if (Input.GetKey(switchKey))
            {
                // Check if max hold duration reached
                if (Time.time - holdStartTime >= maxHoldDuration)
                {
                    DoSwitch();
                }
            }

            // On key release, do the switch to the pending unit
            if (Input.GetKeyUp(switchKey))
            {
                DoSwitch();
            }
        }
    }

    void DoSwitch()
    {
        if (pendingSwitchUnit != null)
        {
            Debug.Log($"Switching guards to {pendingSwitchUnit.name}");
            selectedUnit = pendingSwitchUnit;

            // Update camera target
            CameraScript cameraScript = mainCamera.GetComponent<CameraScript>();
            cameraScript.SetTarget(selectedUnit.transform);
        }
        CancelHold();
    }

    void CancelHold()
    {
        isHoldingSwitch = false;
        pendingSwitchUnit = null;
        holdStartTime = 0f;
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
