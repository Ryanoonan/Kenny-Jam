using UnityEngine;
using System.Collections.Generic;

public class GameManagerScript : MonoBehaviour
{
    public PlayerManagerScript playerManager;
    public float delayBeforeStart = 1f;

    public float requiredDistanceToDropItem = 4f; // Distance to drop an item

    private bool gameLoopStarted = false;

    public GameObject prisonerObj;

    public float baseGameDuration = 10f;      // in seconds

    private float gameDuration; // Total game duration, adjusted for items
    public float timePerItem = 10f;
    public TMPro.TextMeshProUGUI timerText;
    private float gameTimer = 0f;
    private float preGameTimer = 0f;

    private int numberOfItems = 0; // Number of items to collect, can be set in the Inspector

    private List<ControllableUnit> allUnits = new List<ControllableUnit>();

    void Start()
    {
        gameDuration = baseGameDuration;
        // Optionally collect all units at the start
        ControllableUnit[] units = FindObjectsByType<ControllableUnit>(FindObjectsSortMode.None);
        allUnits.AddRange(units);

        CameraScript camScript = Camera.main.GetComponent<CameraScript>();
        camScript.SetTarget(prisonerObj.transform);

    }




    void Update()
    {
        // Wait until the delay has passed
        if (!gameLoopStarted)
        {
            preGameTimer += Time.deltaTime;

            if (preGameTimer >= delayBeforeStart && Input.GetKeyDown(KeyCode.E))
            {
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
        gameDuration = baseGameDuration + (timePerItem * numberOfItems);
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

    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(gameTimer).ToString();
        }
    }

    public void ItemDropped(InteractableItem item)
    {
        if ((item.transform.position - prisonerObj.transform.position).magnitude < requiredDistanceToDropItem)
        {
            numberOfItems++;
            gameDuration = baseGameDuration + (timePerItem * numberOfItems);
            // Remove the InteractableItem component from the item
            Destroy(item.GetComponent<InteractableItem>());

            // Move the item GameObject to the prisoner's position
            item.transform.position = prisonerObj.transform.position;
            ResetGameLoop(); // Reset the game loop to apply the new duration
        }
    }

}
