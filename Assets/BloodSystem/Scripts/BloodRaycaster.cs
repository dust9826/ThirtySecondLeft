using UnityEngine;

namespace BloodSystem
{
    /// <summary>
    /// 피 분산을 위한 Raycast 유틸리티
    /// </summary>
    public static class BloodRaycaster
    {
        /// <summary>
        /// 충돌 지점에서 여러 방향으로 Ray를 발사하여 피를 분산시킵니다
        /// </summary>
        /// <param name="origin">시작 위치</param>
        /// <param name="mainDirection">메인 방향 (충돌 힘의 반대 방향)</param>
        /// <param name="force">충돌 힘 (Ray 길이에 영향)</param>
        /// <param name="rayCount">발사할 Ray 개수 (5~15 권장)</param>
        /// <param name="layerMask">충돌 검사할 레이어</param>
        /// <param name="splatSize">피 스플래터 크기</param>
        /// <param name="minDistance">최소 Ray 거리</param>
        /// <param name="maxDistance">최대 Ray 거리</param>
        public static void Cast(
            Vector2 origin,
            Vector2 mainDirection,
            float force,
            int rayCount,
            LayerMask layerMask,
            float splatSize = 0.3f,
            float minDistance = 0.1f,
            float maxDistance = 2f)
        {
            if (BloodManager.Instance == null)
            {
                Debug.LogWarning("BloodRaycaster: BloodManager 인스턴스가 없습니다!");
                return;
            }

            if (rayCount <= 0)
                return;

            // 부채꼴 비율 (60-70%)
            float coneRatio = Random.Range(0.6f, 0.7f);
            int coneRays = Mathf.RoundToInt(rayCount * coneRatio);
            int randomRays = rayCount - coneRays;

            // Ray 길이는 힘에 비례
            float normalizedForce = Mathf.Clamp01(force / 10f);
            float rayLength = Mathf.Lerp(minDistance, maxDistance, normalizedForce);

            // 메인 방향 정규화
            Vector2 mainDir = mainDirection.normalized;

            // 부채꼴 각도 (±60°)
            float coneAngle = 60f;

            // 부채꼴 Ray들
            for (int i = 0; i < coneRays; i++)
            {
                float angle = Random.Range(-coneAngle, coneAngle);
                Vector2 direction = Rotate(mainDir, angle);
                float randomizedLength = rayLength * Random.Range(0.5f, 1.5f);
                CastSingleRay(origin, direction, randomizedLength, layerMask, splatSize);
            }

            // 랜덤 Ray들
            for (int i = 0; i < randomRays; i++)
            {
                float angle = Random.Range(0f, 360f);
                Vector2 direction = Rotate(Vector2.right, angle);
                float randomizedLength = rayLength * Random.Range(0.3f, 1.2f);
                CastSingleRay(origin, direction, randomizedLength, layerMask, splatSize);
            }
        }

        private static void CastSingleRay(Vector2 origin, Vector2 direction, float length, LayerMask layerMask, float splatSize)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, length, layerMask);

            if (hit.collider != null)
            {
                // 피 추가
                BloodManager.Instance.AddBloodAtPoint(hit.point, splatSize);

                // 디버그 시각화 (옵션)
                #if UNITY_EDITOR
                Debug.DrawLine(origin, hit.point, Color.red, 2f);
                #endif
            }
            else
            {
                // 디버그 시각화 (옵션)
                #if UNITY_EDITOR
                Debug.DrawRay(origin, direction * length, Color.yellow, 2f);
                #endif
            }
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        #region 오버로드

        /// <summary>
        /// ContactPoint2D를 사용한 간편한 Cast
        /// </summary>
        public static void CastFromContact(
            ContactPoint2D contact,
            float force,
            int rayCount,
            LayerMask layerMask,
            float splatSize = 0.3f)
        {
            // 충돌 반대 방향 = 힘이 튕겨나가는 방향
            Vector2 mainDirection = -contact.normal;
            Cast(contact.point, mainDirection, force, rayCount, layerMask, splatSize);
        }

        #endregion
    }
}
