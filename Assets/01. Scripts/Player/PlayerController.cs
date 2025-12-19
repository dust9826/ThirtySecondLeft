using UnityEngine;
using UnityEngine.InputSystem; // 반드시 추가

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    private Animator animator;
    private Rigidbody2D rb;
    private float moveX;
    private bool isGrounded;

    // 넉백 상태를 제어하기 위한 변수
    [HideInInspector] public bool isKnockback = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    
    void OnMove(InputValue value)
    {
        Vector2 inputVector = value.Get<Vector2>();
        moveX = inputVector.x;
        
        bool isMoving = Mathf.Abs(moveX) > 0f;
        animator.SetBool("isRun", isMoving);
    }
    
    void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void FixedUpdate()
    {
        // 넉백 상태일 때는 이동 입력 무시
        if (isKnockback) return;

        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
    }
}