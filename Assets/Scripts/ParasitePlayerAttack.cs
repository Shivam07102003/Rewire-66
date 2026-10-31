using UnityEngine;
using System.Reflection;

public class ParasitePlayerAttack : MonoBehaviour
{
    [Header("Damage")]
    public float damageInterval = 1f;
    public int damageAmount = 1;

    [Header("Repair Panels")]
    public RepairPanel[] repairPanels;

    [Header("Movement Slow")]
    [Range(0.05f, 1f)]
    public float movementMultiplier = 0.35f;

    private GameObject player;
    private Rigidbody2D playerBody;
    private RobotSolarRecharge recharge;

    private float nextDamageTime;
    private Vector3 lastPlayerPosition;
    private bool hasPreviousPosition;

    private void OnTriggerStay2D(Collider2D other)
    {
        Transform target = other.transform;

        while (target != null && !target.CompareTag("Player"))
            target = target.parent;

        if (target == null)
            return;

        player = target.gameObject;

        if (playerBody == null)
            playerBody = player.GetComponent<Rigidbody2D>();

        if (recharge == null)
            recharge = player.GetComponent<RobotSolarRecharge>();

        if (!hasPreviousPosition)
        {
            lastPlayerPosition = player.transform.position;
            hasPreviousPosition = true;
        }

        if (recharge != null)
            recharge.NotifyInterrupted();

        InterruptNearbyTasks();

        if (Time.time >= nextDamageTime)
        {
            nextDamageTime = Time.time + damageInterval;
            DamagePlayer();
        }
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 currentPosition = player.transform.position;
        Vector3 movement = currentPosition - lastPlayerPosition;

        player.transform.position =
            lastPlayerPosition + movement * movementMultiplier;

        lastPlayerPosition = player.transform.position;

        if (playerBody != null)
            playerBody.linearVelocity *= movementMultiplier;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform target = other.transform;

        while (target != null && !target.CompareTag("Player"))
            target = target.parent;

        if (target != null && target.gameObject == player)
        {
            player = null;
            playerBody = null;
            recharge = null;
            hasPreviousPosition = false;
            lastPlayerPosition = Vector3.zero;
        }
    }

    private void InterruptNearbyTasks()
    {
        RepairPanel[] panels =
            FindObjectsByType<RepairPanel>();

        foreach (RepairPanel panel in panels)
        {
            if (panel != null && panel.IsPlayerNearby)
                panel.InterruptRepair();
        }

        if (repairPanels != null)
        {
            foreach (RepairPanel panel in repairPanels)
            {
                if (panel != null && panel.IsPlayerNearby)
                    panel.InterruptRepair();
            }
        }
    }

    private void DamagePlayer()
    {
        if (player == null)
            return;

        MonoBehaviour[] components =
            player.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component == null)
                continue;

            if (component is RobotSolarRecharge)
                continue;

            if (TryCallDamageMethod(component))
                return;
        }
    }

    private bool TryCallDamageMethod(MonoBehaviour component)
    {
        string[] methodNames =
        {
            "TakeDamage",
            "ApplyDamage",
            "Damage",
            "ReceiveDamage"
        };

        BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        foreach (string methodName in methodNames)
        {
            MethodInfo method =
                component.GetType().GetMethod(
                    methodName,
                    flags,
                    null,
                    new[] { typeof(int) },
                    null
                );

            if (method != null)
            {
                method.Invoke(
                    component,
                    new object[] { damageAmount }
                );

                return true;
            }
        }

        return false;
    }
}