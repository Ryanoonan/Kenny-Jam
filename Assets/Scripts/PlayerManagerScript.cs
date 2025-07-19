using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

public class PlayerManagerScript : MonoBehaviour
{
    public GameObject spaceBar;              // Assign this in Inspector
    public Camera mainCamera;                // Assign this in Inspector
    public float switchDistance = 2f;        // How close you need to be to switch guards
    public KeyCode switchKey = KeyCode.Space;    // Key to press to switch guards

    public GameObject eKey;                 // Assign this in Inspector

    public KeyCode pickUpKey = KeyCode.E; // Key to press to pick up items


    public ControllableUnit selectedUnit;   // The currently controlled guard
    private List<ControllableUnit> allUnits = new List<ControllableUnit>();  // All guards in the scene

    private float holdStartTime = 0f;
    private ControllableUnit pendingSwitchUnit = null;
    private bool isHoldingSwitch = false;
    private float maxHoldDuration = 3f;

    public bool gameStarted;

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
            }
        }
        else
        {
        }
        gameStarted = true;
    }

    // Update runs every frame
    void Update()
    {

        if (!gameStarted || selectedUnit == null) return;
        HandleSwitch();

        if (selectedUnit.currentItem != null)
        {
            HandleDrop();
        }
        else
        {
            HandlePickUp();
        }
    }

    void FixedUpdate()
    {
        if (!gameStarted) return;
        HandleMovement();
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
        else
        {
            selectedUnit.SetVelocity(Vector3.zero); // Stop movement if no input
        }
    }

    void HandleSwitch()
    {
        if (!gameStarted || selectedUnit == null)
        {
            spaceBar.SetActive(false);
            return;
        }

        // Only run this if selectedUnit is not null
        SpaceScript spaceScript = spaceBar.GetComponent<SpaceScript>();

        if (!isHoldingSwitch)
        {
            ControllableUnit nearest = FindNearestSwitchableUnit();

            if (nearest != null)
            {
                spaceBar.SetActive(true);
                spaceScript.SetNearestUnit(nearest);

                if (Input.GetKeyDown(switchKey))
                {
                    isHoldingSwitch = true;
                    holdStartTime = Time.time;
                    pendingSwitchUnit = nearest;
                }
            }
            else
            {
                spaceBar.SetActive(false);
            }
        }
        else
        {
            // Same as before: check hold duration or key release
            if (Input.GetKey(switchKey) && Time.time - holdStartTime >= maxHoldDuration)
            {
                DoSwitch();
            }

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
            // Stop the previous unit's movement
            if (selectedUnit != null)
            {
                selectedUnit.SetControlled(false);
                selectedUnit.ForceStop(); // <-- new method you'll add
            }

            selectedUnit = pendingSwitchUnit;
            selectedUnit.SetControlled(true);

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

        if (selectedUnit == null) return null;

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


    // Look for a nearby interactable item and pick it up on key press
    void HandlePickUp()
    {
        if (selectedUnit == null) return;
        InteractableItem nearest = FindNearestInteractableItem();
        EKeyScript eKeyScript = eKey.GetComponent<EKeyScript>();
        if (nearest != null)
        {
            eKeyScript.SetNearestItem(nearest);
            eKey.SetActive(true);

            // Check if the pickup key is pressed
            if (Input.GetKeyDown(pickUpKey))
            {

                // Trigger pickup on selected unit
                selectedUnit.PickUpItem(nearest);
            }
        }
        else
        {
            eKey.SetActive(false);
        }

        // Finds the closest InteractableItem that is within range
        InteractableItem FindNearestInteractableItem()
        {
            InteractableItem nearest = null;
            float minDistance = Mathf.Infinity;

            InteractableItem[] items = FindObjectsByType<InteractableItem>(FindObjectsSortMode.None);

            foreach (var item in items)
            {
                float distance = Vector3.Distance(selectedUnit.transform.position, item.transform.position);
                if (distance <= switchDistance && distance < minDistance)
                {
                    minDistance = distance;
                    nearest = item;
                }
            }

            return nearest;
        }
    }

    // Handle dropping the currently held item
    void HandleDrop()
    {
        if (selectedUnit == null) return;

        EKeyScript eKeyScript = eKey.GetComponent<EKeyScript>();
        eKey.SetActive(true);
        // Note: You may want to modify EKeyScript to show "Drop" instead of item name

        // Check if the drop key is pressed
        if (Input.GetKeyDown(pickUpKey))
        {

            // Trigger drop on selected unit
            selectedUnit.DropItem();


            eKey.SetActive(false);
        }
    }

    /// <summary>
    /// Called when a ControllableUnit is spotted by a Field of View
    /// </summary>
    /// <param name="spottedUnit">The unit that was spotted</param>
    public void unitSpotted(ControllableUnit spottedUnit)
    {
        if (spottedUnit == selectedUnit && selectedUnit.currentItem != null)
        {
            // If the spotted unit is the currently controlled unit and it has an item, do nothing
            Debug.Log("You LOST!!");
        }
    }
}