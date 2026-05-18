using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Visual Flip")]
    [SerializeField] private Transform visualRoot;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private Animator animator;

    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 moveInput;
    private bool jumpQueued;
    private bool isGrounded;

    private bool facingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();

        CacheActions();
    }

    private void OnEnable()
    {
        CacheActions();

        if (jumpAction != null)
            jumpAction.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        if (jumpAction != null)
            jumpAction.performed -= OnJumpPerformed;
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        CheckGrounded();

        HandleFlip();

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        ApplyHorizontalMovement();
        ApplyJump();
        ClampFallSpeed();
    }

    private void ApplyHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void ApplyJump()
    {
        if (!jumpQueued || !isGrounded)
        {
            jumpQueued = false;
            return;
        }

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        jumpQueued = false;
        isGrounded = false;
    }

    private void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                -maxFallSpeed
            );
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck == null) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void HandleFlip()
    {
        if (moveInput.x > 0 && !facingRight)
            Flip();

        else if (moveInput.x < 0 && facingRight)
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = visualRoot.localScale;
        scale.x *= -1;
        visualRoot.localScale = scale;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("Grounded", isGrounded);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpQueued = true;
    }

    private void CacheActions()
    {
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}