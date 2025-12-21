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
        // 넉백 상태 확인 (밀리기만 해도 true라고 가정)
        bool isKnockBack = knockbackable != null && knockbackable.IsKnockBack;
        animator.SetBool("IsKnockback", isKnockBack);

        if (isKnockBack)
        {
            // 1순위: 넉백 중이면 진행 중인 공격 코루틴을 강제 종료
            if (isAttacking)
            {
                StopAllCoroutines(); 
                isAttacking = false;
            }
            return; // 넉백 중에는 아래의 이동/공격 로직을 아예 실행하지 않음
        }

        // 2순위 & 3순위 로직
        if (!isAttacking)
        {
            DetectPlayer();
            if (target != null)
            {
                if (isPlayerInAttackRange) TryAttack();
                else if (isPlayerInRange) FollowPlayer();
            }
            else
            {
                // 타겟이 없으면 멈춤
                animator.SetBool("IsRun", false);
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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
    
        // 1. 확실하게 멈추기
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("IsRun", false);
    
        // 2. 공격 애니메이션 시작 (여기서 딱 한 번만 호출)
        animator.SetTrigger("IsAttack");

        // 3. 선딜레이: 애니메이션에서 무기를 휘두르는 타이밍까지 대기
        yield return new WaitForSeconds(0.8f); 

        // 4. 실제 발사체 생성 함수 호출 (내부에 트리거 제거됨)
        SpawnProjectile();

        // 5. 후딜레이: 공격 동작이 마무리될 때까지 대기
        yield return new WaitForSeconds(0.5f); 

        isAttacking = false;
    }

    void SpawnProjectile() // 기존 Attack 함수 수정
    {
        if (projectilePrefab != null && firePoint != null && target != null)
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