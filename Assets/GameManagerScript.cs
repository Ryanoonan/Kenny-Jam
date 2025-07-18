using UnityEngine;
using System.Collections.Generic;

public class GameManagerScript : MonoBehaviour
{
    public PlayerManagerScript playerManager;
    public float delayBeforeStart = 1f;

    private bool gameLoopStarted = false;

    public float gameDuration = 30f; // Set in inspector
    public TMPro.TextMeshProUGUI timerText;
    private float gameTimer = 0f;
    private float preGameTimer = 0f;

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
            preGameTimer += Time.deltaTime;

            if (preGameTimer >= delayBeforeStart && Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Game Loop Starting...");
                gameLoopStarted = true;
                gameTimer = gameDuration;
                playerManager.GameLoopStart();
            }

            return;
        }

        // Handle patrols
        HandleUnitPatrols();

        // Update countdown
        gameTimer -= Time.deltaTime;
        UpdateTimerUI();

        if (gameTimer <= 0)
        {
            Debug.Log("Time's up! Resetting...");
            ResetGameLoop();
        }
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
    void ResetGameLoop()
    {
        gameLoopStarted = false;
        preGameTimer = 0f;

        // Optional: reset controlled unit
        playerManager.selectedUnit = null;

        // Reset camera to the prisoner
        GameObject prisonerObj = GameObject.Find("Prisoner");
        if (prisonerObj != null)
        {
            CameraScript camScript = Camera.main.GetComponent<CameraScript>();
            camScript.SetTarget(prisonerObj.transform);
        }

        // Disable all patrols and control
        foreach (var unit in allUnits)
        {
            if (unit != null)
            {
                unit.SetControlled(false);
                unit.transform.position = unit.startPosition;
            }
        }

        Debug.Log("Game loop reset. Waiting to restart...");
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(gameTimer).ToString();
        }
    }

}
