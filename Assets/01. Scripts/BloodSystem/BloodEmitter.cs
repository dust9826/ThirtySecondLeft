using UnityEngine;

namespace BloodSystem
{
    /// <summary>
    /// 피 효과를 발생시키는 트리거 컴포넌트. 적 오브젝트에 부착하여 사용합니다.
    /// </summary>
    public class BloodEmitter : MonoBehaviour
    {
        [Header("Blood Effects")]
        [Tooltip("피 VFX 파티클 Prefab (VFX Graph 또는 Particle System)")]
        [SerializeField] private GameObject bloodVFXPrefab;

        [Tooltip("즉각 스플래터 크기 (충돌 지점)")]
        [SerializeField] private float immediateSplatSize = 0.5f;

        [Header("Raycast Settings")]
        [Tooltip("발사할 Ray 개수 (5~15 권장)")]
        [SerializeField] private int rayCount = 5;

        [Tooltip("Raycast로 생성되는 스플래터 크기")]
        [SerializeField] private float raycastSplatSize = 0.3f;

        [Tooltip("Raycast가 충돌 검사할 레이어")]
        [SerializeField] private LayerMask raycastLayerMask = -1; // Everything

        [Header("Advanced Settings")]
        [Tooltip("Ray 최소/최대 거리")]
        [SerializeField] private float minRayDistance = 2f;
        [SerializeField] private float maxRayDistance = 4f;

        /// <summary>
        /// 피 효과를 발생시킵니다
        /// </summary>
        /// <param name="contactPoint">충돌 지점</param>
        /// <param name="impactForce">충돌 힘 벡터</param>
        public void Emit(Vector2 contactPoint, Vector2 impactForce)
        {
            if (BloodManager.Instance == null)
            {
                Debug.LogWarning("BloodEmitter: BloodManager 인스턴스가 없습니다!");
                return;
            }
            contactPoint -= impactForce * 0.02f;
            // 1. 즉각 스플래터 (충돌 지점에 큰 피 자국)
            EmitImmediateSplat(contactPoint);

            // 2. VFX 파티클 (시각 효과)
            EmitVFXParticles(contactPoint, impactForce);

            // 3. Blood Raycast (환경에 피 분산)
            EmitBloodRays(contactPoint, impactForce);
        }

        /// <summary>
        /// ContactPoint2D를 사용한 간편한 Emit
        /// </summary>
        /// <param name="contact">충돌 정보</param>
        /// <param name="impactForceMagnitude">충돌 힘 크기</param>
        public void EmitFromContact(ContactPoint2D contact, float impactForceMagnitude)
        {
            Vector2 impactForce = -contact.normal * impactForceMagnitude;
            Emit(contact.point, impactForce);
        }

        /// <summary>
        /// Collision2D를 사용한 간편한 Emit (첫 번째 contact 사용)
        /// </summary>
        public void EmitFromCollision(Collision2D collision)
        {
            if (collision.contactCount > 0)
            {
                ContactPoint2D contact = collision.GetContact(0);
                float forceMagnitude = collision.relativeVelocity.magnitude;
                EmitFromContact(contact, forceMagnitude);
            }
        }

        private void EmitImmediateSplat(Vector2 position)
        {
            // 충돌 지점에 즉시 큰 피 스플래터 생성
            BloodManager.Instance.AddBloodAtPoint(position, immediateSplatSize);
        }

        private void EmitVFXParticles(Vector2 position, Vector2 force)
        {
            if (bloodVFXPrefab == null)
                return;

            // 파티클 스폰
            GameObject particleInstance = Instantiate(bloodVFXPrefab, position, Quaternion.identity);

            // Particle System 설정
            var particleSystem = particleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                // 파티클 방향을 Force 방향으로 회전
                var shape = particleSystem.shape;
                if (shape.enabled && shape.shapeType == ParticleSystemShapeType.Cone)
                {
                    // Force 방향으로 Cone 회전
                    float angle = Mathf.Atan2(force.y, force.x) * Mathf.Rad2Deg;
                    particleInstance.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
                }

                // 파티클 재생
                particleSystem.Play();
            }

            // 파티클 수명보다 약간 길게 대기 후 삭제
            Destroy(particleInstance, particleSystem != null ? particleSystem.main.duration + particleSystem.main.startLifetime.constantMax + 1f : 5f);
        }

        private void EmitBloodRays(Vector2 position, Vector2 force)
        {
            if (rayCount <= 0)
                return;

            // 충돌 힘의 반대 방향이 메인 방향 (피가 튀는 방향)
            Vector2 mainDirection = force.normalized;
            float forceMagnitude = force.magnitude;

            // BloodRaycaster를 통해 피 분산
            BloodRaycaster.Cast(
                origin: position,
                mainDirection: mainDirection,
                force: forceMagnitude,
                rayCount: rayCount,
                layerMask: raycastLayerMask,
                splatSize: raycastSplatSize,
                minDistance: minRayDistance,
                maxDistance: maxRayDistance
            );
        }

        #region 디버그

        private void OnDrawGizmosSelected()
        {
            // 즉각 스플래터 범위 시각화
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, immediateSplatSize / 2f);

            // Raycast 범위 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxRayDistance);
        }

        #endregion
    }
}
