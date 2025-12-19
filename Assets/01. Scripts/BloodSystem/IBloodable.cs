using UnityEngine;

namespace BloodSystem
{
    /// <summary>
    /// 피가 묻을 수 있는 오브젝트를 위한 인터페이스
    /// </summary>
    public interface IBloodable
    {
        /// <summary>
        /// 지정된 월드 위치에 피를 추가합니다
        /// </summary>
        /// <param name="worldPos">월드 좌표</param>
        /// <param name="splatTexture">스플래터 텍스처</param>
        /// <param name="size">스플래터 크기 (월드 단위)</param>
        /// <param name="rotation">회전 (라디안)</param>
        void AddBlood(Vector2 worldPos, Texture2D splatTexture, float size, float rotation);

        /// <summary>
        /// 해당 월드 좌표가 이 오브젝트 영역 내에 있는지 확인
        /// </summary>
        /// <param name="worldPos">월드 좌표</param>
        /// <returns>영역 내에 있으면 true</returns>
        bool ContainsWorldPoint(Vector2 worldPos);

        /// <summary>
        /// 피 마스크 RenderTexture를 반환
        /// </summary>
        /// <returns>피 마스크 RenderTexture</returns>
        RenderTexture GetBloodMaskRT();

        /// <summary>
        /// 모든 피를 제거합니다
        /// </summary>
        void ClearBlood();

        /// <summary>
        /// 오브젝트의 월드 공간 Bounds를 반환
        /// </summary>
        /// <returns>월드 Bounds</returns>
        Bounds GetWorldBounds();
    }
}
