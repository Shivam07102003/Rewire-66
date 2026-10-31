using UnityEngine;
using System.Reflection;

public class RobotSolarRecharge : MonoBehaviour
{
    [Header("Healing")]
    public float delayBeforeRecharge = 1f;
    public float healthPerSecond = 4f;
    public float movementThreshold = 0.01f;

    [Header("References")]
    public Rigidbody2D playerBody;
    public MonoBehaviour healthComponent;

    [Header("Task Blocking")]
    public bool taskInProgress = false;
    public KeyCode taskKey = KeyCode.E;
    public bool holdingTaskKeyBlocksHealing = true;

    [Header("Parasite Attack Blocking")]
    public float attackStopTime = 0.25f;

    private Vector3 lastPosition;
    private float stillTime;
    private float lastParasiteAttackTime = -Mathf.Infinity;
    private float integerHealRemainder;

    private void Awake()
    {
        if (playerBody == null)
            playerBody = GetComponent<Rigidbody2D>();

        lastPosition = transform.position;
    }

    private void Update()
    {
        float distanceMoved =
            Vector3.Distance(transform.position, lastPosition);

        bool isMovingByTransform =
            distanceMoved > movementThreshold;

        bool isMovingByRigidbody = false;

        if (playerBody != null)
        {
            isMovingByRigidbody =
                playerBody.linearVelocity.sqrMagnitude >
                movementThreshold * movementThreshold;
        }

        lastPosition = transform.position;

        // Running or moving stops healing immediately.
        if (isMovingByTransform || isMovingByRigidbody)
        {
            stillTime = 0f;
            return;
        }

        // A parasite attack stops healing.
        if (Time.time - lastParasiteAttackTime < attackStopTime)
        {
            stillTime = 0f;
            return;
        }

        // Any active task stops healing.
        if (IsDoingTask())
        {
            stillTime = 0f;
            return;
        }

        // The robot must remain stationary for one full second.
        stillTime += Time.deltaTime;

        if (stillTime < delayBeforeRecharge)
            return;

        RestoreHealth(healthPerSecond * Time.deltaTime);
    }

    private bool IsDoingTask()
    {
        if (taskInProgress)
            return true;

        if (holdingTaskKeyBlocksHealing &&
            Input.GetKey(taskKey))
        {
            return true;
        }

        return false;
    }

    public void NotifyInterrupted()
    {
        lastParasiteAttackTime = Time.time;
        stillTime = 0f;
    }

    public void BeginTask()
    {
        taskInProgress = true;
        stillTime = 0f;
    }

    public void EndTask()
    {
        taskInProgress = false;
        stillTime = 0f;
    }

    public void SetTaskInProgress(bool value)
    {
        taskInProgress = value;

        if (value)
            stillTime = 0f;
    }

    private void RestoreHealth(float amount)
    {
        if (healthComponent == null)
            return;

        string[] methodNames =
        {
            "RestoreHealth",
            "Heal",
            "AddHealth",
            "IncreaseHealth",
            "RestoreEnergy",
            "AddEnergy"
        };

        BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        foreach (string methodName in methodNames)
        {
            MethodInfo floatMethod =
                healthComponent.GetType().GetMethod(
                    methodName,
                    flags,
                    null,
                    new[] { typeof(float) },
                    null
                );

            if (floatMethod != null)
            {
                floatMethod.Invoke(
                    healthComponent,
                    new object[] { amount }
                );

                return;
            }
        }

        string[] currentValueNames =
        {
            "currentHealth",
            "health",
            "currentHP",
            "hp",
            "currentEnergy",
            "energy",
            "currentEnergyValue"
        };

        string[] maximumValueNames =
        {
            "maxHealth",
            "maximumHealth",
            "maxHP",
            "maximumHP",
            "maxEnergy",
            "maximumEnergy"
        };

        FieldInfo currentField = null;
        FieldInfo maximumField = null;

        foreach (string name in currentValueNames)
        {
            currentField =
                healthComponent.GetType().GetField(name, flags);

            if (currentField != null)
                break;
        }

        foreach (string name in maximumValueNames)
        {
            maximumField =
                healthComponent.GetType().GetField(name, flags);

            if (maximumField != null)
                break;
        }

        if (currentField == null || maximumField == null)
            return;

        float currentValue =
            System.Convert.ToSingle(
                currentField.GetValue(healthComponent)
            );

        float maximumValue =
            System.Convert.ToSingle(
                maximumField.GetValue(healthComponent)
            );

        if (currentValue >= maximumValue)
        {
            integerHealRemainder = 0f;
            return;
        }

        if (currentField.FieldType == typeof(int))
        {
            integerHealRemainder += amount;

            int wholeAmount =
                Mathf.FloorToInt(integerHealRemainder);

            if (wholeAmount <= 0)
                return;

            integerHealRemainder -= wholeAmount;

            int newValue =
                Mathf.Min(
                    Mathf.RoundToInt(currentValue) + wholeAmount,
                    Mathf.RoundToInt(maximumValue)
                );

            currentField.SetValue(
                healthComponent,
                newValue
            );
        }
        else
        {
            float newValue =
                Mathf.Min(
                    currentValue + amount,
                    maximumValue
                );

            currentField.SetValue(
                healthComponent,
                newValue
            );
        }
    }
}