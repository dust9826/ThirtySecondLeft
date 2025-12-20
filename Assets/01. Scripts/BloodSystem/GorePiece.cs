using System.Collections;
using UnityEngine;

namespace BloodSystem
{
    /// <summary>
    /// 개별 팔다리 파편의 동작을 관리합니다.
    /// Rigidbody2D로 날아가며 피를 뿌리고, 충돌 시 스플래터를 생성합니다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class GorePiece : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem bloodTrail;

        [Header("Trail Settings")]
        [Tooltip("피를 뿌리는 시간")]
        [SerializeField] private float trailDuration = 2f;

        [Tooltip("이 속도 이하면 피 멈춤")]
        [SerializeField] private float minVelocityForTrail = 0.5f;

        [Header("Lifetime Settings")]
        [Tooltip("총 수명")]
        [SerializeField] private float lifetime = 10f;

        [Tooltip("페이드 시작 시간")]
        [SerializeField] private float fadeStartTime = 8f;

        [Header("Splatter Settings")]
        [Tooltip("충돌 시 스플래터 생성")]
        [SerializeField] private bool splatOnCollision = true;

        [Tooltip("스플래터 크기 범위")]
        [SerializeField] private Vector2 splatSizeRange = new Vector2(0.2f, 0.5f);

        [Tooltip("최대 스플래터 수")]
        [SerializeField] private int maxSplatCount = 3;

        private Rigidbody2D rb;
        private int currentSplatCount = 0;
        private bool isTrailActive = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// 파편을 발사합니다.
        /// </summary>
        /// <param name="force">발사 힘</param>
        /// <param name="torque">회전력</param>
        public void Launch(Vector2 force, float torque)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
            rb.AddTorque(torque, ForceMode2D.Impulse);

            if (bloodTrail != null)
            {
                bloodTrail.Play();
                isTrailActive = true;
            }

            StartCoroutine(LifetimeRoutine());
        }

        private IEnumerator LifetimeRoutine()
        {
            float elapsed = 0f;
            float trailElapsed = 0f;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;

                // Trail 관리
                if (isTrailActive)
                {
                    trailElapsed += Time.deltaTime;

                    // 속도가 느려지거나 시간이 지나면 Trail 정지
                    if (trailElapsed >= trailDuration || rb.linearVelocity.magnitude < minVelocityForTrail)
                    {
                        StopTrail();
                    }
                }

                // 페이드 아웃
                if (elapsed >= fadeStartTime && spriteRenderer != null)
                {
                    float fadeProgress = (elapsed - fadeStartTime) / (lifetime - fadeStartTime);
                    Color color = spriteRenderer.color;
                    color.a = 1f - fadeProgress;
                    spriteRenderer.color = color;
                }

                yield return null;
            }

            // 수명 종료 - 오브젝트 제거
            Destroy(gameObject);
        }

        private void StopTrail()
        {
            if (bloodTrail != null && isTrailActive)
            {
                bloodTrail.Stop();
                isTrailActive = false;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!splatOnCollision || currentSplatCount >= maxSplatCount || collision.gameObject.layer == LayerMask.NameToLayer("Gore"))
                return;

            // 충돌 지점에 작은 스플래터 생성
            ContactPoint2D contact = collision.GetContact(0);

            if (BloodManager.Instance != null)
            {
                float size = Random.Range(splatSizeRange.x, splatSizeRange.y);
                BloodManager.Instance.AddBloodAtPoint(contact.point, size);
            }
            
            currentSplatCount++;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
