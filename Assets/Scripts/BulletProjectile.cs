using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [Header("Bullet")]
    public Vector2 direction;
    public float speed = 12f;
    public int damage = 1;
    public float lifetime = 2f;

    [Header("Parasite Hit")]
    public float hitDistance = 0.5f;

    [Header("Wall Collision")]
    public LayerMask wallLayers;
    public float wallCheckRadius = 0.05f;

    private bool alreadyHit;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (alreadyHit)
            return;

        Vector2 oldPosition =
            transform.position;

        Vector2 movement =
            direction.normalized *
            speed *
            Time.deltaTime;

        float movementDistance =
            movement.magnitude;

        Vector2 moveDirection =
            movement.normalized;

        if (HitWall(
            oldPosition,
            moveDirection,
            movementDistance
        ))
        {
            DestroyBullet();
            return;
        }

        Vector2 newPosition =
            oldPosition + movement;

        ParasiteHealth[] parasites =
            FindObjectsByType<ParasiteHealth>();

        foreach (ParasiteHealth parasite in parasites)
        {
            if (parasite == null)
                continue;

            Vector2 parasitePosition =
                parasite.transform.position;

            Vector2 closestPoint =
                GetClosestPointOnLine(
                    oldPosition,
                    newPosition,
                    parasitePosition
                );

            float distance =
                Vector2.Distance(
                    closestPoint,
                    parasitePosition
                );

            if (distance <= hitDistance)
            {
                alreadyHit = true;

                Debug.Log(
                    "Bullet hit parasite: " +
                    parasite.name
                );

                parasite.TakeDamage(damage);

                DestroyBullet();
                return;
            }
        }

        transform.position =
            newPosition;
    }

    private bool HitWall(
        Vector2 origin,
        Vector2 direction,
        float distance
    )
    {
        int layerMask =
            wallLayers.value == 0
                ? Physics2D.AllLayers
                : wallLayers.value;

        RaycastHit2D[] hits =
            Physics2D.CircleCastAll(
                origin,
                wallCheckRadius,
                direction,
                distance,
                layerMask
            );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            Transform hitRoot =
                hit.collider.transform.root;

            if (hitRoot == transform.root)
                continue;

            if (hit.collider.isTrigger)
                continue;

            if (hitRoot.CompareTag("Player"))
                continue;

            if (hitRoot.GetComponentInParent<ParasiteHealth>() != null)
                continue;

            Debug.Log(
                "Bullet hit wall: " +
                hit.collider.name
            );

            return true;
        }

        return false;
    }

    private void DestroyBullet()
    {
        alreadyHit = true;
        Destroy(gameObject);
    }

    private Vector2 GetClosestPointOnLine(
        Vector2 lineStart,
        Vector2 lineEnd,
        Vector2 point
    )
    {
        Vector2 line =
            lineEnd - lineStart;

        float lineLengthSquared =
            line.sqrMagnitude;

        if (lineLengthSquared == 0f)
            return lineStart;

        float t =
            Vector2.Dot(
                point - lineStart,
                line
            ) / lineLengthSquared;

        t =
            Mathf.Clamp01(t);

        return lineStart + line * t;
    }
}