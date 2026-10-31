using System.Collections.Generic;
using UnityEngine;

public class InventoryHUD : MonoBehaviour
{
    public PlayerInventory inventory;

    public Vector2 margin =
        new Vector2(24f, 24f);

    private void Awake()
    {
        if (inventory == null)
            inventory =
                GetComponentInParent<PlayerInventory>();
    }

    private void OnGUI()
    {
        if (inventory == null)
            return;

        float panelWidth = 235f;
        float panelHeight = 300f;

        Rect panelRect = new Rect(
            16f,
            500f,
            panelWidth,
            panelHeight
        );

        GUI.color =
            new Color(0f, 0f, 0f, 0.82f);

        GUI.Box(panelRect, "");

        GUI.color = Color.white;

        GUIStyle titleStyle =
            new GUIStyle(GUI.skin.label);

        titleStyle.fontSize = 18;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;

        GUI.Label(
            new Rect(
                panelRect.x + 18f,
                panelRect.y + 12f,
                250f,
                30f
            ),
            "INVENTORY",
            titleStyle
        );

        GUIStyle itemStyle =
            new GUIStyle(GUI.skin.label);

        itemStyle.fontSize = 14;
        itemStyle.normal.textColor =
            Color.white;

        float y = panelRect.y + 52f;

        DrawLine(
            panelRect,
            ref y,
            "FUSES: " +
            inventory.fuseCount,
            itemStyle,
            Color.white
        );

        DrawLine(
            panelRect,
            ref y,
            "COOLANT CANISTERS: " +
            inventory.coolantCanisterCount,
            itemStyle,
            Color.white
        );

        List<string> circuitBoards =
            GetCircuitBoards();

        DrawLine(
            panelRect,
            ref y,
            "CIRCUIT BOARDS: " +
            circuitBoards.Count,
            itemStyle,
            Color.cyan
        );

        foreach (string board in circuitBoards)
        {
            DrawLine(
                panelRect,
                ref y,
                "  • " +
                FormatItemName(board),
                itemStyle,
                Color.cyan
            );
        }

        List<string> accessCards =
            GetAccessCards();

        DrawLine(
            panelRect,
            ref y,
            "ACCESS CARDS: " +
            accessCards.Count,
            itemStyle,
            Color.green
        );

        foreach (string card in accessCards)
        {
            DrawLine(
                panelRect,
                ref y,
                "  • " +
                FormatItemName(card),
                itemStyle,
                Color.green
            );
        }

        List<string> keys =
            GetKeys();

        DrawLine(
            panelRect,
            ref y,
            "KEYS: " +
            keys.Count,
            itemStyle,
            Color.yellow
        );

        foreach (string key in keys)
        {
            DrawLine(
                panelRect,
                ref y,
                "  • " +
                FormatItemName(key),
                itemStyle,
                Color.yellow
            );
        }
    }

    private void DrawLine(
        Rect panelRect,
        ref float y,
        string text,
        GUIStyle style,
        Color color
    )
    {
        style.normal.textColor = color;

        GUI.Label(
            new Rect(
                panelRect.x + 18f,
                y,
                panelRect.width - 30f,
                24f
            ),
            text,
            style
        );

        y += 24f;
    }

    private List<string> GetCircuitBoards()
    {
        List<string> boards =
            new List<string>();

        foreach (string item
                 in inventory.specificItemIDs)
        {
            if (item.ToLower().Contains("board"))
                boards.Add(item);
        }

        return boards;
    }

    private List<string> GetAccessCards()
    {
        List<string> cards =
            new List<string>();

        foreach (string item
                 in inventory.accessCardIDs)
        {
            if (item.ToLower().Contains("card"))
                cards.Add(item);
        }

        return cards;
    }

    private List<string> GetKeys()
    {
        List<string> keys =
            new List<string>();

        foreach (string item
                 in inventory.keyIDs)
        {
            if (item.ToLower().Contains("key"))
                keys.Add(item);
        }

        return keys;
    }

    private string FormatItemName(string itemID)
    {
        switch (itemID)
        {
            case "StoragePanelBoard":
                return "Storage Panel Circuit Board";

            case "ControlPanelBoard":
                return "Control Panel Circuit Board";

            case "GeneratorAControlBoard":
                return "Generator A Control Board";

            case "GeneratorBBackupBoard":
                return "Generator B Backup Board";

            case "StorageAccessCard":
                return "Storage Access Card";

            case "ControlAccessCard":
                return "Control Access Card";

            case "MaintenanceAccessCard":
                return "Maintenance Access Card";

            case "GenALockerKey":
                return "Generator A Locker Key";

            case "StorageLockerKey":
                return "Storage Locker Key";

            case "GeneratorFenceKey":
                return "Generator Fence Key";

            default:
                return itemID;
        }
    }
}