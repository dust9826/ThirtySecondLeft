using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // 반드시 추가

public class PlayerKick : MonoBehaviour
{
    [Header("Reference")]
    public GameObject arrowIndicator; // 화살표 이미지
    private SpriteRenderer arrowSpriteRenderer; // 화살표 이미지의 SpriteRenderer
    public Transform kickPoint;      // 발차기 판정 중심점
    private Rigidbody2D rb;
    private Camera cam;

    [Header("Settings")] 
    public bool canKick = true;
    public float maxDragDistance = 3f;
    public float kickForce = 15f;      // 발차기 피격된 적에게 주는 힘
    public float recoilForce = 10f;    // 플레이어가 반동으로 밀려나는 힘
    public float kickRadius = 1.5f;    // 발차기 피격 범위
    public float knockbackDuration = 0.5f; // 넉백 지속 시간(반동으로 밀려나는 동안 움직이지못함)
    public LayerMask enemyLayer;       // 적 레이어
    public float arrowOffsetDistance = 2f; // 화살표를 플레이어 중앙에서 2f 떨어진 곳에 배치

    private PlayerController playerController;
    private Animator animator;
    private bool isDragging = false;
    private Vector3 kickDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        
        if (arrowIndicator != null)
        {
            arrowSpriteRenderer = arrowIndicator.GetComponent<SpriteRenderer>();
            arrowSpriteRenderer.enabled = false; // 시작 시 비활성화
        }
    }

    void Update()
    {
        UpdateCharacterDirection(); // 마우스 방향에 따라 플레이어 방향 전환
        
        if (Mouse.current.leftButton.wasPressedThisFrame) StartDrag();
        if (isDragging) ContinueDrag(); // 드래그 중 화살표 위치 및 방향 업데이트
        if (Mouse.current.leftButton.wasReleasedThisFrame) EndDrag();
    }
    
    void UpdateCharacterDirection()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        float directionX = mousePos.x - transform.position.x;

        if (directionX > 0.1f) // 마우스가 오른쪽
        {
            transform.localScale = new Vector3(1, 1, 1);
            arrowIndicator.transform.localScale = new Vector3(1, 1, 1);
        }
        else if (directionX < -0.1f) // 마우스가 왼쪽
        {
            transform.localScale = new Vector3(-1, 1, 1);
            arrowIndicator.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void StartDrag()
    {
        isDragging = true;
        if (arrowSpriteRenderer != null)
        {
            arrowSpriteRenderer.enabled = true; // 드래그 시작 시 화살표 활성화
        }
    }

    void ContinueDrag()
    {
        Vector3 playerPos = transform.position;
        Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        Vector3 rawDir = (Vector3)mousePos - playerPos;
        float dist = Mathf.Min(rawDir.magnitude, maxDragDistance);
        kickDirection = rawDir.normalized; // 발차기 방향 저장 (노멀라이즈된 벡터)

        // 화살표 이미지 위치 및 회전 업데이트
        if (arrowIndicator != null)
        {
            // 플레이어 중앙에서 offsetDistance 만큼 떨어진 곳에 위치
            arrowIndicator.transform.position = playerPos + kickDirection * arrowOffsetDistance;
            
            // 화살표 방향 설정
            float angle = Mathf.Atan2(kickDirection.y, kickDirection.x) * Mathf.Rad2Deg;
            arrowIndicator.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }

    void EndDrag()
    {
        isDragging = false;
        if (arrowSpriteRenderer != null)
        {
            arrowSpriteRenderer.enabled = false; // 드래그 종료 시 화살표 비활성화
        }
        PerformKick();
    }

    void PerformKick()
    {
        canKick = false;
        animator.SetTrigger("doKick");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(kickPoint.position, kickRadius, enemyLayer);
        bool hitSomething = false;

        foreach (Collider2D enemy in hitEnemies)
        {
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.linearVelocity = Vector2.zero;
                enemyRb.AddForce(kickDirection * kickForce, ForceMode2D.Impulse);
                hitSomething = true;
            }
        }

        if (hitSomething)
        {
            canKick = true;
            if (playerController != null)
            {
                playerController.isKnockback = true;
            }

            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = -kickDirection * recoilForce;

            StartCoroutine(ResetKnockback());
        }
    }

    IEnumerator ResetKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration);
        if (playerController != null)
        {
            playerController.isKnockback = false;
        }
    }
}