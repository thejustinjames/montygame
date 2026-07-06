using UnityEngine;

/// <summary>
/// Gentle, forgiving 2D platformer movement for MontyGame.
/// - Arrow keys / A-D to move
/// - Space / Up / W to jump (with coyote-time + jump-buffer grace windows)
/// Ground is detected with a short boxcast so it "just works" on any
/// collider tagged "Ground" (the bootstrap tags the floor and all tiles).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBuffer = 0.12f;

    private Rigidbody2D rb;
    private BoxCollider2D box;
    private float coyoteTimer;
    private float jumpBufferTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Horizontal movement
        float input = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(input * moveSpeed, rb.linearVelocity.y);

        // Jump buffering: remember a jump press for a short window
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            jumpBufferTimer = jumpBuffer;
        else
            jumpBufferTimer -= Time.deltaTime;

        // Coyote time: allow a jump shortly after leaving the ground
        if (IsGrounded())
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Execute jump
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
    }

    bool IsGrounded()
    {
        // Three short rays fired from JUST BELOW the feet (outside our own
        // collider, so we never falsely detect ourselves). Any hit on another
        // collider = grounded. Checking left/center/right handles tile edges.
        Bounds b = box.bounds;
        float y = b.min.y - 0.02f;            // start just below the collider
        float rayLen = 0.15f;

        return GroundRay(new Vector2(b.center.x, y), rayLen)
            || GroundRay(new Vector2(b.min.x + 0.05f, y), rayLen)
            || GroundRay(new Vector2(b.max.x - 0.05f, y), rayLen);
    }

    bool GroundRay(Vector2 origin, float length)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, length);
        return hit.collider != null && hit.collider.gameObject != gameObject;
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        rb.linearVelocity = Vector2.zero;
    }
}
