using UnityEngine;

namespace BloodSystem
{
    /// <summary>
    /// Gore 시스템의 스폰 관리자.
    /// 적이 사망할 때 팔다리 파편과 피 폭발을 생성합니다.
    /// </summary>
    public class GoreSpawner : MonoBehaviour
    {
        public static GoreSpawner Instance { get; private set; }

        [Header("Gore Piece Prefabs")]
        [Tooltip("팔다리 파편 프리팹들 (GorePiece 컴포넌트 필요)")]
        [SerializeField] private GameObject[] gorePiecePrefabs;

        [Header("Particle Effects")]
        [Tooltip("피 폭발 파티클 프리팹")]
        [SerializeField] private ParticleSystem bloodBurstPrefab;

        [Header("Spawn Settings")]
        [Tooltip("최소 파편 개수")]
        [SerializeField] private int minPieces = 3;

        [Tooltip("최대 파편 개수")]
        [SerializeField] private int maxPieces = 6;

        [Tooltip("스폰 위치 랜덤 오프셋")]
        [SerializeField] private float spawnOffset = 0.3f;

        [Header("Force Settings")]
        [Tooltip("파편에 가할 힘 범위")]
        [SerializeField] private Vector2 forceRange = new Vector2(5f, 15f);

        [Tooltip("회전력 범위")]
        [SerializeField] private Vector2 torqueRange = new Vector2(-360f, 360f);

        [Tooltip("퍼지는 각도 (도)")]
        [SerializeField] private float spreadAngle = 120f;

        [Header("Main Splatter")]
        [Tooltip("메인 스플래터 개수")]
        [SerializeField] private int mainSplatterCount = 2;

        [Tooltip("메인 스플래터 크기 범위")]
        [SerializeField] private Vector2 mainSplatterSize = new Vector2(1.5f, 2.5f);

        [Header("Parents")]
        [SerializeField] private Transform goreParent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Spawn(Collision2D collision)
        {
            ContactPoint2D contact = collision.GetContact(0);
            float forceMagnitude = collision.relativeVelocity.magnitude;
            Vector2 impactForce = -contact.normal * forceMagnitude;
            Spawn(contact.point, impactForce * 0.1f);
        }

        /// <summary>
        /// Gore 효과를 스폰합니다.
        /// </summary>
        /// <param name="position">스폰 위치</param>
        /// <param name="impactDirection">충격 방향 (피가 튀는 방향)</param>
        public void Spawn(Vector2 position, Vector2 impactDirection)
        {
            Vector2 mainDir = impactDirection.normalized;
            if (mainDir == Vector2.zero)
                mainDir = Vector2.up;

            // 1. 피 폭발 파티클
            // SpawnBloodBurst(position, mainDir);

            // 2. 메인 스플래터 (큰 크기)
            // SpawnMainSplatters(position);

            // 3. 팔다리 파편
            SpawnGorePieces(position, mainDir);
        }

        private void SpawnBloodBurst(Vector2 position, Vector2 direction)
        {
            if (bloodBurstPrefab == null)
                return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            ParticleSystem burst = Instantiate(bloodBurstPrefab, position, Quaternion.Euler(0, 0, angle - 90f));
            burst.Play();

            // 파티클 수명 후 제거
            float duration = burst.main.duration + burst.main.startLifetime.constantMax + 1f;
            Destroy(burst.gameObject, duration);
        }

        private void SpawnMainSplatters(Vector2 position)
        {
            if (BloodManager.Instance == null)
                return;

            for (int i = 0; i < mainSplatterCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * spawnOffset;
                float size = Random.Range(mainSplatterSize.x, mainSplatterSize.y);
                BloodManager.Instance.AddBloodAtPoint(position + offset, size);
            }
        }

        private void SpawnGorePieces(Vector2 position, Vector2 mainDir)
        {
            if (gorePiecePrefabs == null || gorePiecePrefabs.Length == 0)
                return;

            int pieceCount = Random.Range(minPieces, maxPieces + 1);

            for (int i = 0; i < pieceCount; i++)
            {
                // 랜덤 프리팹 선택
                GameObject prefab = gorePiecePrefabs[Random.Range(1, gorePiecePrefabs.Length)];
                if(i == 0)
                    prefab = gorePiecePrefabs[0];
                if (prefab == null)
                    continue;

                // 스폰 위치 (약간의 오프셋)
                Vector2 spawnPos = position + Random.insideUnitCircle * spawnOffset;

                // 랜덤 방향 (부채꼴 기반)
                Vector2 direction = GetRandomDirection(mainDir, spreadAngle / 2f);

                // 랜덤 힘
                float force = Random.Range(forceRange.x, forceRange.y);
                Vector2 forceVector = direction * force;
                
                // 랜덤 회전력
                float torque = Random.Range(torqueRange.x, torqueRange.y);

                // 파편 생성
                Transform parent = goreParent != null ? goreParent : transform;
                GameObject piece = Instantiate(prefab, spawnPos, Quaternion.identity, parent);

                // GorePiece 컴포넌트로 발사
                GorePiece gorePiece = piece.GetComponent<GorePiece>();
                if (gorePiece != null)
                {
                    gorePiece.Launch(forceVector, torque);
                }
                else
                {
                    // GorePiece가 없으면 Rigidbody2D에 직접 힘 적용
                    Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.AddForce(forceVector, ForceMode2D.Impulse);
                        rb.AddTorque(torque, ForceMode2D.Impulse);
                    }
                }
            }
        }

        /// <summary>
        /// 기준 방향에서 최대 각도 범위 내의 랜덤 방향을 반환합니다.
        /// </summary>
        private Vector2 GetRandomDirection(Vector2 baseDir, float maxAngle)
        {
            float randomAngle = Random.Range(-maxAngle, maxAngle);
            return Rotate(baseDir, randomAngle);
        }

        /// <summary>
        /// 2D 벡터를 회전시킵니다.
        /// </summary>
        private Vector2 Rotate(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 스폰 범위 시각화
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnOffset);

            // 퍼지는 각도 시각화
            Gizmos.color = Color.yellow;
            Vector3 leftDir = Quaternion.Euler(0, 0, spreadAngle / 2f) * Vector3.up;
            Vector3 rightDir = Quaternion.Euler(0, 0, -spreadAngle / 2f) * Vector3.up;
            Gizmos.DrawRay(transform.position, leftDir * 2f);
            Gizmos.DrawRay(transform.position, rightDir * 2f);
        }
#endif
    }
}
