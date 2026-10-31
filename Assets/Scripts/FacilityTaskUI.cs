using System.Collections.Generic;
using UnityEngine;

public class FacilityTaskUI : MonoBehaviour
{
    private readonly Color panelColor =
        new Color(0.01f, 0.025f, 0.05f, 0.94f);

    private readonly Color cyanColor =
        new Color(0f, 0.85f, 1f);

    private readonly Color taskColor =
        Color.white;

    private readonly Color requirementColor =
        Color.yellow;

    private void OnGUI()
    {
        FacilityTaskManager manager =
            FacilityTaskManager.Instance;

        if (manager == null)
            return;

        string currentTask =
            manager.GetCurrentTaskID();

        if (string.IsNullOrWhiteSpace(
                currentTask))
        {
            return;
        }

        string nextTask =
            GetNextTask(
                manager,
                currentTask
            );

        float panelX = 16f;
        float panelY = 250f;
        float panelWidth = 270f;
        float panelHeight =
            string.IsNullOrWhiteSpace(nextTask)
                ? 100f
                : 150f;

        Rect panelRect = new Rect(
            panelX,
            panelY,
            panelWidth,
            panelHeight
        );

        GUI.color = panelColor;
        GUI.Box(panelRect, "");

        GUI.color = cyanColor;

        GUI.DrawTexture(
            new Rect(
                panelRect.x,
                panelRect.y,
                4f,
                panelRect.height
            ),
            Texture2D.whiteTexture
        );

        GUI.DrawTexture(
            new Rect(
                panelRect.x,
                panelRect.y,
                panelRect.width,
                3f
            ),
            Texture2D.whiteTexture
        );

        GUI.color = Color.white;

        GUIStyle headerStyle =
            new GUIStyle(GUI.skin.label);

        headerStyle.fontSize = 15;
        headerStyle.fontStyle =
            FontStyle.Bold;
        headerStyle.normal.textColor =
            cyanColor;

        GUI.Label(
            new Rect(
                panelX + 14f,
                panelY + 10f,
                panelWidth - 25f,
                22f
            ),
            "NEXT OBJECTIVES",
            headerStyle
        );

        DrawTask(
            manager,
            currentTask,
            1,
            panelY + 38f
        );

        if (!string.IsNullOrWhiteSpace(nextTask))
        {
            DrawTask(
                manager,
                nextTask,
                2,
                panelY + 92f
            );
        }

        GUI.color = Color.white;
    }

    private void DrawTask(
        FacilityTaskManager manager,
        string taskID,
        int taskNumber,
        float y
    )
    {
        string taskName =
            GetDisplayName(taskID);

        string requirementText =
            GetRequirementsText(
                manager,
                taskID
            );

        GUIStyle taskStyle =
            new GUIStyle(GUI.skin.label);

        taskStyle.fontSize = 14;
        taskStyle.fontStyle =
            FontStyle.Bold;
        taskStyle.normal.textColor =
            taskColor;

        GUI.Label(
            new Rect(
                30f,
                y,
                245f,
                22f
            ),
            taskNumber +
            ". " +
            taskName.ToUpper(),
            taskStyle
        );

        GUIStyle requirementStyle =
            new GUIStyle(GUI.skin.label);

        requirementStyle.fontSize = 12;
        requirementStyle.wordWrap = true;
        requirementStyle.normal.textColor =
            requirementColor;

        GUI.Label(
            new Rect(
                42f,
                y + 22f,
                230f,
                30f
            ),
            "Required: " +
            requirementText,
            requirementStyle
        );
    }

    private string GetNextTask(
        FacilityTaskManager manager,
        string currentTask
    )
    {
        for (int i = 0;
             i < manager.GetTaskCount();
             i++)
        {
            if (!string.Equals(
                    manager.GetTaskIDAt(i),
                    currentTask))
            {
                continue;
            }

            if (i + 1 >= manager.GetTaskCount())
                return string.Empty;

            return manager.GetTaskIDAt(i + 1);
        }

        return string.Empty;
    }

    private string GetRequirementsText(
        FacilityTaskManager manager,
        string taskID
    )
    {
        List<string> requirements =
            manager.GetRequirementsForTask(
                taskID
            );

        if (requirements == null ||
            requirements.Count == 0)
        {
            return "No component assigned";
        }

        List<string> names =
            new List<string>();

        foreach (string requirement
                 in requirements)
        {
            names.Add(
                GetDisplayName(requirement)
            );
        }

        return string.Join(
            ", ",
            names
        );
    }

    private string GetDisplayName(
        string value
    )
    {
        switch (value)
        {
            case "EntryRoomTrial":
                return "Entry Room Trial";

            case "CentralHub":
                return "Central Hub Repair";

            case "StorageWire":
                return "Storage Wire Repair";

            case "Control":
                return "Control Room Repair";

            case "Maintenance":
                return "Maintenance Room Repair";

            case "Storage":
                return "Storage Room Repair";

            case "LoadingBay":
                return "Loading Bay Repair";

            case "GeneratorARepair1":
                return "Generator A Repair 1";

            case "GeneratorBRepair1":
                return "Generator B Repair 1";

            case "GeneratorARepair2":
                return "Generator A Repair 2";

            case "GeneratorBRepair2":
                return "Generator B Repair 2";

            case "GeneratorAFinal":
                return "Generator A Final Repair";

            case "CommTowerActivation":
                return "Activate Communication Tower";

            case "ControlPanelBoard":
                return "Control Panel Circuit Board";

            case "StoragePanelBoard":
                return "Storage Panel Circuit Board";

            case "GeneratorAControlBoard":
                return "Generator A Control Board";

            case "GeneratorBBackupBoard":
                return "Generator B Backup Board";

            case "GenALockerKey":
                return "Generator A Locker Key";

            case "StorageLockerKey":
                return "Storage Locker Key";

            case "GeneratorFenceKey":
                return "Generator Fence Key";

            case "Fuse":
                return "Fuse";

            case "Coolant Canister":
                return "Coolant Canister";

            case "Wire Set":
                return "Wire Set";

            default:
                return value;
        }
    }
}