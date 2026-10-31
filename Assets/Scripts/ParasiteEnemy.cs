using System.Collections.Generic;
using UnityEngine;

public class ParasiteEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float bodyRadius = 0.65f;
    public LayerMask wallLayers;

    [Header("Starting Area")]
    public float followRadius = 24f;

    [Header("Gate")]
    public Collider2D centralControlGateCollider;

    [Header("Visual Facing")]
    public Transform visualToRotate;
    public bool smoothFacing = true;
    public float facingSpeed = 12f;

    [Header("Attack")]
    public float attackDistance = 0.7f;
    public float damageAmount = 10f;
    public float attackCooldown = 1f;

    private Transform player;
    private PlayerHealth playerHealth;

    private Vector2 startingPosition;
    private bool playerOutsideStartingArea;

    private float nextAttackTime;

    private int currentRoom;
    private int targetRoom;
    private int routeStage;

    private bool enteredCurrentRoom;

    private List<int> roomRoute =
        new List<int>();

    private enum Room
    {
        Entry,
        Central,
        Storage,
        Maintenance,
        Control,
        LoadingBay,
        GeneratorA,
        GeneratorB
    }

    private class Door
    {
        public int roomA;
        public int roomB;

        public Vector2 pointA;
        public Vector2 middlePoint;
        public Vector2 pointB;

        public bool requiresGate;

        public Door(
            int roomA,
            int roomB,
            Vector2 pointA,
            Vector2 middlePoint,
            Vector2 pointB,
            bool requiresGate
        )
        {
            this.roomA = roomA;
            this.roomB = roomB;
            this.pointA = pointA;
            this.middlePoint = middlePoint;
            this.pointB = pointB;
            this.requiresGate = requiresGate;
        }

        public Vector2 GetPointForRoom(int room)
        {
            return room == roomA ? pointA : pointB;
        }

        public int GetOtherRoom(int room)
        {
            return room == roomA ? roomB : roomA;
        }
    }

    private readonly List<Door> doors =
        new List<Door>();

    void Start()
    {
        startingPosition =
            transform.position;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            playerHealth =
                playerObject.GetComponent<PlayerHealth>();
        }

        // Entry ↔ Central Hub
        doors.Add(new Door(
            (int)Room.Entry,
            (int)Room.Central,
            new Vector2(4f, 0),
            new Vector2(5f, 0),
            new Vector2(6f, 0),
            false
        ));

        // Central Hub ↔ Storage
        doors.Add(new Door(
            (int)Room.Central,
            (int)Room.Storage,
            new Vector2(16.5f, 5.25f),
            new Vector2(16.5f, 7f),
            new Vector2(16.5f, 8.75f),
            false
        ));

        // Central Hub ↔ Maintenance
        doors.Add(new Door(
            (int)Room.Central,
            (int)Room.Maintenance,
            new Vector2(16.5f, -5.25f),
            new Vector2(16.5f, -7f),
            new Vector2(16.5f, -8.75f),
            false
        ));

        // Central Hub ↔ Control
        doors.Add(new Door(
            (int)Room.Central,
            (int)Room.Control,
            new Vector2(21.25f, 0),
            new Vector2(22f, 0),
            new Vector2(23.75f, 0),
            true
        ));

        // Storage ↔ Loading Bay
        doors.Add(new Door(
            (int)Room.Storage,
            (int)Room.LoadingBay,
            new Vector2(35.25f, 14),
            new Vector2(37f, 14),
            new Vector2(38.75f, 14),
            false
        ));

        // Control ↔ Generator A
        doors.Add(new Door(
            (int)Room.Control,
            (int)Room.GeneratorA,
            new Vector2(32.25f, 0),
            new Vector2(34f, 0),
            new Vector2(35.75f, 0),
            false
        ));

        // Maintenance ↔ Generator B
        doors.Add(new Door(
            (int)Room.Maintenance,
            (int)Room.GeneratorB,
            new Vector2(32.25f, -13.5f),
            new Vector2(34f, -13.5f),
            new Vector2(35.75f, -13.5f),
            false
        ));

        currentRoom =
            FindRoom(transform.position);

        if (player != null)
        {
            targetRoom =
                FindRoom(player.position);

            BuildRoute();
        }
    }

    void Update()
    {
        if (player == null)
            return;
        
        UpdateVisualFacing();

        float distanceFromStart =
            Vector2.Distance(
                player.position,
                startingPosition
            );

        // Stop following when the player leaves this parasite's area.
        if (distanceFromStart > followRadius)
        {
            roomRoute.Clear();
            routeStage = 0;
            enteredCurrentRoom = false;
            playerOutsideStartingArea = true;

            return;
        }

        // Rebuild the route when the player returns.
        if (playerOutsideStartingArea)
        {
            playerOutsideStartingArea = false;

            currentRoom =
                FindRoom(transform.position);

            targetRoom =
                FindRoom(player.position);

            BuildRoute();
        }

        // Once inside the correct room, chase directly.
        if (enteredCurrentRoom)
        {
            float distanceToPlayer =
                Vector2.Distance(
                    transform.position,
                    player.position
                );

            if (distanceToPlayer <= attackDistance)
            {
                AttackPlayer();
                return;
            }

            bool moved =
                MoveSafely(player.position);

            if (!moved)
            {
                enteredCurrentRoom = false;

                currentRoom =
                    FindRoom(transform.position);

                targetRoom =
                    FindRoom(player.position);

                BuildRoute();
            }

            return;
        }

        int newTargetRoom =
            FindRoom(player.position);

        if (newTargetRoom != targetRoom)
        {
            targetRoom = newTargetRoom;
            BuildRoute();
        }

        if (currentRoom == targetRoom)
        {
            enteredCurrentRoom = true;
            return;
        }

        FollowRoute();
    }

    void FollowRoute()
    {
        if (roomRoute.Count == 0)
            return;

        int nextRoom =
            roomRoute[0];

        Door door =
            FindDoor(currentRoom, nextRoom);

        if (door == null)
        {
            BuildRoute();
            return;
        }

        Vector2 targetPoint;

        if (routeStage == 0)
        {
            targetPoint =
                door.GetPointForRoom(currentRoom);
        }
        else if (routeStage == 1)
        {
            targetPoint =
                door.middlePoint;
        }
        else
        {
            targetPoint =
                door.GetPointForRoom(nextRoom);
        }

        bool gateClosed =
            door.requiresGate &&
            centralControlGateCollider != null &&
            centralControlGateCollider.enabled;

        // Stop at the current side of the locked gate.
        if (gateClosed)
        {
            routeStage = 0;

            MoveSafely(
                door.GetPointForRoom(currentRoom)
            );

            return;
        }

        float distanceToTarget =
            Vector2.Distance(
                transform.position,
                targetPoint
            );

        if (distanceToTarget > 0.4f)
        {
            MoveSafely(targetPoint);
            return;
        }

        if (routeStage < 2)
        {
            routeStage++;
            return;
        }

        currentRoom = nextRoom;
        roomRoute.RemoveAt(0);
        routeStage = 0;
        enteredCurrentRoom = roomRoute.Count == 0;
    }

    void BuildRoute()
    {
        roomRoute.Clear();
        routeStage = 0;
        enteredCurrentRoom = false;

        if (currentRoom == targetRoom)
        {
            enteredCurrentRoom = true;
            return;
        }

        int[] previous =
            new int[8];

        for (int i = 0; i < previous.Length; i++)
            previous[i] = -1;

        Queue<int> queue =
            new Queue<int>();

        queue.Enqueue(currentRoom);
        previous[currentRoom] = currentRoom;

        while (queue.Count > 0)
        {
            int room =
                queue.Dequeue();

            foreach (Door door in doors)
            {
                if (door.roomA != room &&
                    door.roomB != room)
                {
                    continue;
                }

                int nextRoom =
                    door.GetOtherRoom(room);

                if (previous[nextRoom] != -1)
                    continue;

                previous[nextRoom] = room;
                queue.Enqueue(nextRoom);
            }
        }

        if (previous[targetRoom] == -1)
            return;

        int current =
            targetRoom;

        while (current != currentRoom)
        {
            roomRoute.Insert(0, current);
            current = previous[current];
        }
    }

    int FindRoom(Vector2 position)
    {
        float x = position.x;
        float y = position.y;

        if (y > 6f)
        {
            if (x < 37f)
                return (int)Room.Storage;

            return (int)Room.LoadingBay;
        }

        if (y < -6f)
        {
            if (x < 34f)
                return (int)Room.Maintenance;

            return (int)Room.GeneratorB;
        }

        if (x < 5f)
            return (int)Room.Entry;

        if (x < 23f)
            return (int)Room.Central;

        if (x < 35f)
            return (int)Room.Control;

        return (int)Room.GeneratorA;
    }

    Door FindDoor(int roomA, int roomB)
    {
        foreach (Door door in doors)
        {
            bool matchingPair =
                (door.roomA == roomA &&
                 door.roomB == roomB) ||
                (door.roomA == roomB &&
                 door.roomB == roomA);

            if (matchingPair)
                return door;
        }

        return null;
    }

    bool MoveSafely(Vector2 targetPosition)
    {
        Vector2 currentPosition =
            transform.position;

        Vector2 direction =
            targetPosition - currentPosition;

        if (direction.magnitude < 0.01f)
            return true;

        direction.Normalize();

        float movementDistance =
            moveSpeed * Time.deltaTime;

        Vector2 movement =
            direction * movementDistance;

        Vector2 directPosition =
            currentPosition + movement;

        // Normal direct movement.
        if (!IsSolidAt(directPosition))
        {
            transform.position =
                directPosition;

            return true;
        }

        // If blocked, try sliding vertically along the wall.
        Vector2 verticalMovement =
            new Vector2(0f, movement.y);

        if (Mathf.Abs(verticalMovement.y) > 0.001f &&
            !IsSolidAt(currentPosition + verticalMovement))
        {
            transform.position =
                currentPosition + verticalMovement;

            return true;
        }

        // If vertical sliding is blocked, try sliding horizontally.
        Vector2 horizontalMovement =
            new Vector2(movement.x, 0f);

        if (Mathf.Abs(horizontalMovement.x) > 0.001f &&
            !IsSolidAt(currentPosition + horizontalMovement))
        {
            transform.position =
                currentPosition + horizontalMovement;

            return true;
        }

        return false;
    }

    bool IsSolidAt(Vector2 position)
    {
        int layerMask =
            wallLayers.value == 0
                ? Physics2D.AllLayers
                : wallLayers.value;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                position,
                bodyRadius,
                layerMask
            );

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.transform.root == transform.root)
                continue;

            if (hit.GetComponentInParent<ParasiteEnemy>() != null)
                continue;

            if (hit.transform.root.CompareTag("Player"))
                continue;

            if (hit.isTrigger)
                continue;

            return true;
        }

        return false;
    }
    private void UpdateVisualFacing()
    {
        if (player == null)
            return;

        Transform visual =
            visualToRotate != null
                ? visualToRotate
                : transform;

        Vector2 direction =
            player.position - visual.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        // The artwork faces downward by default.
        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg + 90f;

        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, angle);

        if (smoothFacing)
        {
            visual.rotation =
                Quaternion.Lerp(
                    visual.rotation,
                    targetRotation,
                    facingSpeed * Time.deltaTime
                );
        }
        else
        {
            visual.rotation = targetRotation;
        }
    }
    void AttackPlayer()
    {
        if (Time.time < nextAttackTime)
            return;

        if (playerHealth != null)
            playerHealth.TakeDamage(damageAmount);

        nextAttackTime =
            Time.time + attackCooldown;
    }
}