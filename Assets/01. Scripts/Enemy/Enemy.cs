using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Collision Settings")]
    public float minSpeedToExplode = 5f;
    // 넉백이 끝났다고 판단할 속도 임계값 (0.5 이하로 떨어지면 다시 움직임)
    public float knockbackThreshold = 0.5f; 

    [Header("Detection Settings")]
    public float detectionRange = 7f;
    public float attackRange = 1.5f;
    public LayerMask playerLayer;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    
    [Header("Attack Settings")]
    public GameObject projectilePrefab; 
    public Transform firePoint;         
    public float attackCooldown = 2f;
    public float projectileSpeed = 10f;
    public float projectileLifeTime = 2f;

    private Rigidbody2D rb;
    private Transform target;
    private Animator animator;
    private float lastAttackTime;
    private bool isPlayerInRange;
    private bool isPlayerInAttackRange;
    
    // 넉백 상태 (외부 PlayerKick에서 true로 만들어줌)
    public bool isKnockBack;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 넉백 중이 아닐 때만 AI 로직 실행
        if (!isKnockBack)
        {
            DetectPlayer();

            if (target != null)
            {
                if (isPlayerInAttackRange)
                {
                    TryAttack();
                }
                else if (isPlayerInRange)
                {
                    FollowPlayer();
                }
            }
        }
    }

    void FixedUpdate()
    {
        // 넉백 상태일 때 속도 체크
        if (isKnockBack)
        {
            // 리지드바디의 현재 속도가 임계값보다 낮아지면
            if (rb.linearVelocity.magnitude <= knockbackThreshold)
            {
                // 다시 행동 가능한 상태로 복구
                isKnockBack = false;
            }
        }
    }

    void DetectPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        
        if (playerCollider != null)
        {
            target = playerCollider.transform;
            isPlayerInRange = true;
            
            float dist = Vector2.Distance(transform.position, target.position);
            isPlayerInAttackRange = dist <= attackRange;
        }
        else
        {
            target = null;
            isPlayerInRange = false;
            isPlayerInAttackRange = false;
        }
    }

    void FollowPlayer()
    {
        animator.SetBool("IsRun", true);
        Vector2 direction = (target.position - transform.position).normalized;
        
        if (direction.x > 0.1f) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else if (direction.x < -0.1f) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    void TryAttack()
    {
        // 공격 중에는 이동 애니메이션 정지
        animator.SetBool("IsRun", false);
        
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            animator.SetTrigger("IsAttack");
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Vector2 shootDir = (target.position - firePoint.position).normalized;
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = shootDir * projectileSpeed;
            }
            Destroy(projectile, projectileLifeTime);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy otherEnemy = collision.gameObject.GetComponent<Enemy>();
        if (otherEnemy == null) return;

        float mySpeed = rb.linearVelocity.magnitude;
        float otherSpeed = otherEnemy.rb.linearVelocity.magnitude;

        if (mySpeed >= minSpeedToExplode || otherSpeed >= minSpeedToExplode)
        {
            otherEnemy.Explode();
            Explode();
        }
    }

    public void Explode()
    {
        Destroy(gameObject);
    }
    
}