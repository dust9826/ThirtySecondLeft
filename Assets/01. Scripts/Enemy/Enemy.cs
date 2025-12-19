using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
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
    private Knockbackable knockbackable;
    private float lastAttackTime;
    private bool isPlayerInRange;
    private bool isPlayerInAttackRange;
    private bool isAttacking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        knockbackable = GetComponent<Knockbackable>();
    }

    void Update()
    {
        bool isKnockBack = knockbackable != null && knockbackable.IsKnockBack;

        // 공격 중이 아니고, 넉백 중이 아닐 때만 로직 실행
        if (!isKnockBack && !isAttacking)
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
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackRoutine());
            lastAttackTime = Time.time;
        }
    }
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // 정지
        animator.SetBool("IsRun", false);
        animator.SetTrigger("IsAttack");

        // 실제 발사체 생성 (애니메이션 타이밍에 맞추고 싶다면 중간에 대기 추가)
        Attack();

        // 애니메이션이 끝날 때까지 대기 (예: 0.5초 또는 애니메이션 길이만큼)
        // 혹은 특정 시간 뒤에 다시 움직일 수 있게 함
        yield return new WaitForSeconds(0.8f); 

        isAttacking = false;
    }
    void Attack()
    {
        animator.SetTrigger("IsAttack");
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
    
}