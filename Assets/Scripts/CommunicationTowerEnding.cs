using System.Collections.Generic;
using UnityEngine;

public class CommunicationTowerEnding : MonoBehaviour
{
        public static bool FinalRoomLockActive
        {
            get
            {
                return FinalRoomLockState.IsLocked;
            }

            private set
            {
                FinalRoomLockState.IsLocked = value;
            }
        }
    private enum EndingState
    {
        Inactive,
        SignalStarting,
        Survival,
        Victory,
        GameOver
    }

    [Header("Facility Task")]
    public string taskID = "CommTowerActivation";

    [Header("Tower Visual")]
    public SpriteRenderer towerRenderer;
    public Sprite inactiveTowerSprite;
    public Sprite activeTowerSprite;

    [Header("Parasite Trap")]
    public FakeComponentTrap fakeComponentTrap;

    [Header("Ending Settings")]
    public float signalMessageDuration = 2f;
    public float survivalDuration = 30f;

    [Header("Generator Hall Gates")]
    public Collider2D[] exitGateColliders;

    [Header("Optional Warning Sound")]
    public AudioSource warningLoop;

    [Header("Player")]
    public PlayerHealth playerHealth;

    private EndingState endingState =
        EndingState.Inactive;

    private bool playerNearby;
    private float signalTimer;
    private float survivalTimer;

    private List<GameObject> endingParasites =
        new List<GameObject>();

    private void Start()
    {
        FinalRoomLockActive = false;

        endingState =
            EndingState.Inactive;

        if (towerRenderer != null &&
            inactiveTowerSprite != null)
        {
            towerRenderer.sprite =
                inactiveTowerSprite;
        }

        if (warningLoop != null)
            warningLoop.Stop();
    }

    private void Update()
    {
        if (endingState == EndingState.Inactive)
        {
            HandleTowerActivation();
            return;
        }

        if (endingState == EndingState.SignalStarting)
        {
            signalTimer -= Time.deltaTime;

            if (signalTimer <= 0f)
                BeginSurvivalPhase();

            return;
        }

        if (endingState == EndingState.Survival)
        {
            UpdateSurvivalPhase();
            return;
        }
    }
    private void HandleTowerActivation()
    {
        if (!playerNearby)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (FacilityTaskManager.Instance == null)
        {
            Debug.LogError(
                "FacilityTaskManager is missing."
            );

            return;
        }

        if (!FacilityTaskManager.Instance
            .CanStartTask(taskID))
        {
            Debug.Log(
                FacilityTaskManager.Instance
                    .GetTaskLockMessage(taskID)
            );

            return;
        }

        ActivateCommunicationTower();
    }

    private void ActivateCommunicationTower()
    {
        endingState =
            EndingState.SignalStarting;

        signalTimer =
            signalMessageDuration;

        FinalRoomLockActive = true;

        CloseGeneratorHallExits();

        if (towerRenderer != null &&
            activeTowerSprite != null)
        {
            towerRenderer.sprite =
                activeTowerSprite;
        }

        FacilityTaskManager.Instance
            .CompleteTask(taskID);

        Debug.Log(
            "Communication tower activated."
        );
    }
    private void BeginSurvivalPhase()
    {
        if (fakeComponentTrap == null)
        {
            Debug.LogError(
                "FakeComponentTrap is not assigned."
            );

            return;
        }

        endingParasites =
            fakeComponentTrap
                .TriggerFromExternalSource();

        survivalTimer = 0f;

        endingState =
            EndingState.Survival;

        if (warningLoop != null)
            warningLoop.Play();

        Debug.Log(
            "Final survival phase started. " +
            endingParasites.Count +
            " parasites spawned."
        );
    }

    private void UpdateSurvivalPhase()
    {
        if (playerHealth != null &&
            playerHealth.currentHealth <= 0f)
        {
            TriggerGameOver();
            return;
        }

        survivalTimer += Time.deltaTime;

        if (survivalTimer < survivalDuration)
            return;

        if (!AllEndingParasitesDefeated())
            return;

        TriggerVictory();
    }

    private bool AllEndingParasitesDefeated()
    {
        if (endingParasites == null ||
            endingParasites.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < endingParasites.Count; i++)
        {
            if (endingParasites[i] != null)
                return false;
        }

        return true;
    }

    private void CloseGeneratorHallExits()
    {
        if (exitGateColliders == null)
            return;

        for (int i = 0; i < exitGateColliders.Length; i++)
        {
            if (exitGateColliders[i] != null)
                exitGateColliders[i].enabled = true;
        }
    }

    private void TriggerVictory()
    {
        if (endingState != EndingState.Survival)
            return;

        endingState =
            EndingState.Victory;

        FinalRoomLockState.IsLocked = false;
        GameEndScreen.ShowVictory();

        if (warningLoop != null)
            warningLoop.Stop();

        Debug.Log(
            "VICTORY: Rescue transmission completed."
        );
    }

    private void TriggerGameOver()
    {
        if (endingState == EndingState.GameOver)
            return;

        endingState =
            EndingState.GameOver;

        if (warningLoop != null)
            warningLoop.Stop();

        Debug.Log(
            "GAME OVER: Robot disabled."
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory foundInventory =
            other.GetComponentInParent<PlayerInventory>();

        if (foundInventory == null)
            return;

        playerNearby = true;

        if (playerHealth == null)
        {
            playerHealth =
                other.GetComponentInParent<PlayerHealth>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInventory foundInventory =
            other.GetComponentInParent<PlayerInventory>();

        if (foundInventory == null)
            return;

        playerNearby = false;
    }

        private void OnGUI()
    {
        GUIStyle style =
            new GUIStyle(GUI.skin.label);

        style.alignment =
            TextAnchor.MiddleCenter;

        style.fontStyle =
            FontStyle.Bold;

        style.normal.textColor =
            Color.white;

        if (endingState == EndingState.Inactive &&
            playerNearby)
        {
            GUI.Label(
                new Rect(
                    Screen.width / 2f - 250f,
                    Screen.height - 150f,
                    500f,
                    40f
                ),
                "PRESS E TO ACTIVATE COMMUNICATION TOWER",
                style
            );
        }

        if (endingState == EndingState.SignalStarting)
        {
            style.fontSize = 26;

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 350f,
                    55f,
                    700f,
                    55f
                ),
                "SIGNAL SENT — PREPARE FOR PARASITE ATTACK",
                style
            );

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 300f,
                    115f,
                    600f,
                    40f
                ),
                "SURVIVE FOR 30 SECONDS",
                style
            );
        }

        if (endingState == EndingState.Survival)
        {
            style.fontSize = 28;
            style.normal.textColor =
                Color.red;

            float remainingTime =
                Mathf.Max(
                    0f,
                    survivalDuration - survivalTimer
                );

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 300f,
                    35f,
                    600f,
                    45f
                ),
                "WARNING: PARASITE SWARM",
                style
            );

            style.normal.textColor =
                Color.white;

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 200f,
                    85f,
                    400f,
                    45f
                ),
                "SURVIVE: " +
                Mathf.CeilToInt(remainingTime) +
                " SECONDS",
                style
            );
        }

        if (endingState == EndingState.Victory)
        {
            style.fontSize = 42;
            style.normal.textColor =
                Color.green;

            GUI.Label(
                new Rect(
                    Screen.width / 2f - 350f,
                    Screen.height / 2f - 50f,
                    700f,
                    80f
                ),
                "RE:WIRE 66 COMPLETE",
                style
            );
        }
    }
}