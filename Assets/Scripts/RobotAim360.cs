using UnityEngine;

public class RobotAim360 : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        Vector3 mouseWorldPosition =
            playerCamera.ScreenToWorldPoint(Input.mousePosition);

        mouseWorldPosition.z = transform.position.z;

        Vector2 direction = mouseWorldPosition - transform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // The artwork faces downward by default.
        transform.rotation =
            Quaternion.Euler(0f, 0f, angle + 90f);
    }
}