using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RobotHeadlightController : MonoBehaviour
{
    public Light2D headlight;
    public Camera playerCamera;

    [Header("Direction")]
    public float angleOffset = 0f;

    private void Awake()
    {
        if (headlight == null)
            headlight = GetComponent<Light2D>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        SetHeadlight(false);
    }

    private void Update()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            return;

        Vector3 mousePosition =
            playerCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction =
            mousePosition - transform.position;

        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle + angleOffset
            );
    }

    public void SetHeadlight(bool enabled)
    {
        if (headlight != null)
            headlight.enabled = enabled;
    }
}