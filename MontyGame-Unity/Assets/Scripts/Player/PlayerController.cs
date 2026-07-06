using UnityEngine;

/// <summary>
/// Handles player movement and platforming physics.
/// Gentle, forgiving design: no instant death, bounce-back on obstacles.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float groundDrag = 5f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.1f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float groundedTimer;
    private float jumpBufferTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody2D component!");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        UpdateTimers();
    }

    void HandleMovement()
    {
        float input = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(input * moveSpeed, rb.velocity.y);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBuffer;
        }

        if (jumpBufferTimer > 0 && groundedTimer > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferTimer = 0;
            groundedTimer = 0;
        }
    }

    void UpdateTimers()
    {
        groundedTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundedTimer = coyoteTime;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundedTimer = 0;
        }
    }

    public Vector3 GetPosition() => transform.position;

    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
        rb.velocity = Vector2.zero;
    }
}
