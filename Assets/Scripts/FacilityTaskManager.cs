using UnityEngine;
using System;
using System.Collections.Generic;

public class FacilityTaskManager : MonoBehaviour
{
    public static FacilityTaskManager Instance;

    [Header("Testing")]
    public bool unlockAllTasksForTesting;

    [Header("Current Progress")]
    [SerializeField]
    private int currentTaskIndex = 0;

    private readonly string[] taskOrder =
    {
        "EntryRoomTrial",
        "CentralHub",
        "StorageWire",
        "Control",
        "Maintenance",
        "Storage",
        "LoadingBay",
        "GeneratorARepair1",
        "GeneratorBRepair1",
        "GeneratorARepair2", 
        "GeneratorBRepair2",
        "GeneratorAFinal",
        "CommTowerActivation"
    };

    private readonly HashSet<string> completedTasks =
        new HashSet<string>(
            StringComparer.OrdinalIgnoreCase
        );

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool CanStartTask(string taskID)
    {
        string cleanTaskID =
            NormalizeTaskID(taskID);

        if (string.IsNullOrEmpty(cleanTaskID))
            return false;

        if (!IsKnownTask(cleanTaskID))
        {
            Debug.LogWarning(
                "Unknown task ID: " + taskID
            );

            return false;
        }

        if (unlockAllTasksForTesting)
            return true;

        if (completedTasks.Contains(cleanTaskID))
            return false;

        if (currentTaskIndex >= taskOrder.Length)
            return false;

        string currentTask =
            taskOrder[currentTaskIndex];

        bool canStart =
            string.Equals(
                cleanTaskID,
                currentTask,
                StringComparison.OrdinalIgnoreCase
            );

        if (!canStart)
        {
            Debug.Log(
                "Task locked: " +
                cleanTaskID +
                ". Current task is: " +
                currentTask
            );
        }

        return canStart;
    }

    public void CompleteTask(string taskID)
    {
        string cleanTaskID =
            NormalizeTaskID(taskID);

        if (string.IsNullOrEmpty(cleanTaskID))
            return;

        if (!IsKnownTask(cleanTaskID))
        {
            Debug.LogWarning(
                "Cannot complete unknown task: " +
                taskID
            );

            return;
        }

        if (completedTasks.Contains(cleanTaskID))
            return;

        completedTasks.Add(cleanTaskID);

        AdvanceTaskOrder();

        Debug.Log(
            "Task completed: " +
            cleanTaskID
        );

        if (cleanTaskID == "GeneratorAFinal")
        {
            Debug.Log(
                "REWIRE 66 completed."
            );
        }
    }

    private void AdvanceTaskOrder()
    {
        while (currentTaskIndex < taskOrder.Length)
        {
            string currentTask =
                taskOrder[currentTaskIndex];

            if (!completedTasks.Contains(currentTask))
                break;

            currentTaskIndex++;
        }

        if (currentTaskIndex < taskOrder.Length)
        {
            Debug.Log(
                "Next task: " +
                taskOrder[currentTaskIndex]
            );
        }
        else
        {
            Debug.Log(
                "All facility tasks completed."
            );
        }
    }

    public bool IsTaskComplete(string taskID)
    {
        string cleanTaskID =
            NormalizeTaskID(taskID);

        if (string.IsNullOrEmpty(cleanTaskID))
            return false;

        return completedTasks.Contains(cleanTaskID);
    }

    public string GetCurrentTaskID()
    {
        if (currentTaskIndex >= taskOrder.Length)
            return string.Empty;

        return taskOrder[currentTaskIndex];
    }

    private bool IsKnownTask(string taskID)
    {
        for (int i = 0; i < taskOrder.Length; i++)
        {
            if (string.Equals(
                    taskOrder[i],
                    taskID,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private string NormalizeTaskID(string taskID)
    {
        if (string.IsNullOrWhiteSpace(taskID))
            return string.Empty;

        return taskID.Trim();
    }
    public int GetTaskCount()
    {
        return taskOrder.Length;
    }

    public string GetTaskIDAt(int index)
    {
        if (index < 0 ||
            index >= taskOrder.Length)
        {
            return string.Empty;
        }

        return taskOrder[index];
    }
    public void ResetAllTasksForTesting()
    {
        completedTasks.Clear();
        currentTaskIndex = 0;

        Debug.Log(
            "Facility task progress reset."
        );
    }
    public string GetTaskLockMessage(string taskID)
    {
        if (unlockAllTasksForTesting)
            return string.Empty;

        int requestedIndex = -1;

        for (int i = 0; i < taskOrder.Length; i++)
        {
            if (string.Equals(
                    taskOrder[i],
                    taskID.Trim(),
                    System.StringComparison.OrdinalIgnoreCase))
            {
                requestedIndex = i;
                break;
            }
        }

        if (requestedIndex < 0 ||
            requestedIndex <= currentTaskIndex)
        {
            return string.Empty;
        }

        switch (taskID.Trim())
        {
            case "Control":
                return "Complete the Central Hub connection task before calibrating the control board.";

            case "Maintenance":
                return "Complete the Control Room task before beginning maintenance.";

            case "Storage":
                return "Complete the Maintenance task before restoring storage systems.";

            case "LoadingBay":
                return "Complete the Storage task before entering the Loading Bay sequence.";

            default:
                return "Complete " +
                    taskOrder[currentTaskIndex] +
                    " before starting this task.";
        }
    }

    public List<string> GetRequirementsForTask(
        string requestedTaskID
    )
    {
        List<string> requirements =
            new List<string>();

RepairPanel[] repairPanels =
    FindObjectsByType<RepairPanel>();

        foreach (RepairPanel panel in repairPanels)
        {
            if (!string.Equals(
                    panel.taskID,
                    requestedTaskID,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string requirement = "";

            switch (panel.requiredComponent)
            {
                case RepairPanel.RequiredComponent.Fuse:
                    requirement = "Fuse";
                    break;

                case RepairPanel.RequiredComponent.CoolantCanister:
                    requirement = "Coolant Canister";
                    break;

                case RepairPanel.RequiredComponent.SpecificItem:
                    requirement = panel.requiredItemID;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(requirement) &&
                !requirements.Contains(requirement))
            {
                requirements.Add(requirement);
            }
        }

        WireRepairTask[] wireTasks =
            FindObjectsByType<WireRepairTask>();

        foreach (WireRepairTask wireTask in wireTasks)
        {
            if (!string.Equals(
                    wireTask.taskID,
                    requestedTaskID,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(
                    wireTask.requiredWireSetID) &&
                !requirements.Contains(
                    wireTask.requiredWireSetID))
            {
                requirements.Add(
                    wireTask.requiredWireSetID
                );
            }
        }

        LockedCompartment[] compartments =
            FindObjectsByType<LockedCompartment>();

        foreach (LockedCompartment compartment in compartments)
        {
            if (!string.Equals(
                    compartment.taskID,
                    requestedTaskID,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(
                    compartment.requiredKeyID) &&
                !requirements.Contains(
                    compartment.requiredKeyID))
            {
                requirements.Add(
                    compartment.requiredKeyID
                );
            }
        }

        return requirements;
    }


}

