using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float deceleration = 100f;
    [SerializeField] private float airControlMultiplier = 0.75f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 moveInput;
    private bool jumpQueued;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        CacheActions();
    }

    private void OnEnable()
    {
        CacheActions();

        if (jumpAction == null)
        {
            enabled = false;
            return;
        }

        jumpAction.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
        }
    }

    private void Update()
    {
        if (moveAction == null)
        {
            return;
        }

        moveInput = moveAction.ReadValue<Vector2>();
        CheckGrounded();
    }

    private void FixedUpdate()
    {
        ApplyHorizontalMovement();
        ApplyJump();
        ClampFallSpeed();
    }

    private void ApplyHorizontalMovement()
    {
        float targetSpeed = moveInput.x * moveSpeed;
        float speedDifference = targetSpeed - rb.linearVelocity.x;

        float control = isGrounded ? 1f : airControlMultiplier;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float movement = speedDifference * rate * control * Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);
    }

    private void ApplyJump()
    {
        if (!jumpQueued || !isGrounded)
        {
            jumpQueued = false;
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpQueued = false;
        isGrounded = false;
    }

    private void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpQueued = true;
    }

    private void CacheActions()
    {
        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("PlayerMovement requires a PlayerInput component with an assigned Actions asset.", this);
            enabled = false;
            return;
        }

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

        if (moveAction == null || jumpAction == null)
        {
            Debug.LogError("PlayerMovement could not find Move and Jump actions in the PlayerInput actions asset.", this);
            enabled = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
