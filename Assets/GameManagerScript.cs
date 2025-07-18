using UnityEngine;
using System.Collections.Generic;

public class GameManagerScript : MonoBehaviour
{
    public PlayerManagerScript playerManager;
    public float delayBeforeStart = 1f;

    private bool gameLoopStarted = false;
    private float timer = 0f;

    private List<ControllableUnit> allUnits = new List<ControllableUnit>();

    void Start()
    {
        // Optionally collect all units at the start
        ControllableUnit[] units = FindObjectsByType<ControllableUnit>(FindObjectsSortMode.None);
        allUnits.AddRange(units);

        GameObject prisonerObj = GameObject.Find("Prisoner");
        if (prisonerObj != null)
        {
            CameraScript camScript = Camera.main.GetComponent<CameraScript>();
            camScript.SetTarget(prisonerObj.transform);
        }

    }

    void Update()
    {
        // Wait until the delay has passed
        if (!gameLoopStarted)
        {
            timer += Time.deltaTime;

            if (timer >= delayBeforeStart && Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Game Loop Starting...");
                gameLoopStarted = true;
                playerManager.GameLoopStart();
            }

            return;
        }

        // Handle patrols
        HandleUnitPatrols();
    }

    void HandleUnitPatrols()
    {
        foreach (ControllableUnit unit in allUnits)
        {
            if (unit == null) continue;

            bool isPlayerControlled = (unit == playerManager.selectedUnit);
            unit.SetControlled(isPlayerControlled);
            // Unit will automatically patrol in Update() if not controlled
        }
    }
}
