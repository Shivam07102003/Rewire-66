using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;

    private Rigidbody2D rb;
    private Vector2 movement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        rb.interpolation =
            RigidbodyInterpolation2D.Interpolate;

        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        movement = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        bool isMoving = movement.sqrMagnitude > 0.01f;

        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetFloat(
                "Speed",
                isMoving ? moveSpeed : 0f
            );
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(
            rb.position +
            movement * moveSpeed * Time.fixedDeltaTime
        );
    }
}