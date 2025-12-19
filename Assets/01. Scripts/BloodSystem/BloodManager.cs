using System.Collections.Generic;
using UnityEngine;

namespace BloodSystem
{
    /// <summary>
    /// 피 시스템의 중앙 관리자. IBloodable 오브젝트들을 등록하고 피를 추가합니다.
    /// </summary>
    public class BloodManager : MonoBehaviour
    {
        public static BloodManager Instance { get; private set; }

        [Header("Splatter Textures")]
        [Tooltip("피 스플래터 텍스처 배열. 랜덤으로 선택됩니다.")]
        [SerializeField] private Texture2D[] splatTextures;

        [Header("Shaders")]
        [Tooltip("SplatBlit 셰이더 (Hidden/BloodSystem/SplatBlit)")]
        [SerializeField] private Shader splatBlitShader;

        private List<IBloodable> bloodables = new List<IBloodable>();
        private Material splatBlitMaterial;

        private void Awake()
        {
            // 싱글톤 설정
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Blit 머티리얼 생성
            InitializeSplatBlitMaterial();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // 머티리얼 정리
            if (splatBlitMaterial != null)
            {
                Destroy(splatBlitMaterial);
            }
        }

        private void InitializeSplatBlitMaterial()
        {
            if (splatBlitShader == null)
            {
                Debug.LogError("BloodManager: SplatBlit Shader가 할당되지 않았습니다!");
                return;
            }

            splatBlitMaterial = new Material(splatBlitShader);
            splatBlitMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        #region IBloodable 등록/해제

        /// <summary>
        /// IBloodable 오브젝트를 등록합니다
        /// </summary>
        public void RegisterBloodable(IBloodable bloodable)
        {
            if (!bloodables.Contains(bloodable))
            {
                bloodables.Add(bloodable);
            }
        }

        /// <summary>
        /// IBloodable 오브젝트를 해제합니다
        /// </summary>
        public void UnregisterBloodable(IBloodable bloodable)
        {
            bloodables.Remove(bloodable);
        }

        #endregion

        #region 피 추가

        /// <summary>
        /// 월드 좌표에 피를 추가합니다. 해당 위치의 모든 IBloodable 오브젝트에 적용됩니다.
        /// </summary>
        /// <param name="worldPos">월드 좌표</param>
        /// <param name="size">피 스플래터 크기 (월드 단위)</param>
        /// <param name="rotation">회전 (라디안). 기본값은 랜덤</param>
        /// <param name="splatIndex">스플래터 텍스처 인덱스. -1이면 랜덤</param>
        public void AddBloodAtPoint(Vector2 worldPos, float size, float rotation = -1f, int splatIndex = -1)
        {
            if (splatTextures == null || splatTextures.Length == 0)
            {
                Debug.LogWarning("BloodManager: 스플래터 텍스처가 할당되지 않았습니다!");
                return;
            }

            // 회전이 지정되지 않았으면 랜덤
            if (rotation < 0)
            {
                rotation = Random.Range(0f, Mathf.PI * 2f);
            }

            // 스플래터 텍스처 선택
            Texture2D splatTexture = GetSplatTexture(splatIndex);
            if (splatTexture == null)
            {
                Debug.LogWarning("BloodManager: 유효한 스플래터 텍스처를 찾을 수 없습니다!");
                return;
            }

            // 해당 위치를 포함하는 모든 IBloodable에 피 추가
            foreach (var bloodable in bloodables)
            {
                if (bloodable.ContainsWorldPoint(worldPos))
                {
                    bloodable.AddBlood(worldPos, splatTexture, size, rotation);
                }
            }
        }

        #endregion

        #region Blit 유틸리티

        /// <summary>
        /// RenderTexture에 스플래터를 Blit합니다
        /// </summary>
        /// <param name="target">대상 RenderTexture</param>
        /// <param name="splatTexture">스플래터 텍스처</param>
        /// <param name="uvCenter">UV 좌표 중심 (0~1)</param>
        /// <param name="uvSize">UV 좌표 크기 (0~1)</param>
        /// <param name="rotation">회전 (라디안)</param>
        public void BlitSplat(RenderTexture target, Texture2D splatTexture, Vector2 uvCenter, Vector2 uvSize, float rotation)
        {
            if (splatBlitMaterial == null)
            {
                Debug.LogError("BloodManager: SplatBlit Material이 초기화되지 않았습니다!");
                return;
            }

            // 셰이더 프로퍼티 설정
            splatBlitMaterial.SetTexture("_SplatTex", splatTexture);
            splatBlitMaterial.SetVector("_SplatRect", new Vector4(uvCenter.x, uvCenter.y, uvSize.x, uvSize.y));
            splatBlitMaterial.SetFloat("_SplatRotation", rotation);

            // 임시 RenderTexture 생성 (현재 상태 복사)
            RenderTexture temp = RenderTexture.GetTemporary(target.width, target.height, 0, target.format);
            Graphics.Blit(target, temp);

            // 스플래터 Blit
            Graphics.Blit(temp, target, splatBlitMaterial);

            // 정리
            RenderTexture.ReleaseTemporary(temp);
        }

        #endregion

        #region 텍스처 유틸리티

        /// <summary>
        /// 스플래터 텍스처를 가져옵니다
        /// </summary>
        /// <param name="index">인덱스. -1이면 랜덤</param>
        /// <returns>스플래터 텍스처</returns>
        public Texture2D GetSplatTexture(int index = -1)
        {
            if (splatTextures == null || splatTextures.Length == 0)
                return null;

            if (index < 0 || index >= splatTextures.Length)
            {
                // 랜덤 선택
                index = Random.Range(0, splatTextures.Length);
            }

            return splatTextures[index];
        }

        /// <summary>
        /// 랜덤 스플래터 텍스처를 가져옵니다
        /// </summary>
        public Texture2D GetRandomSplatTexture()
        {
            return GetSplatTexture(-1);
        }

        #endregion

        #region 전체 초기화

        /// <summary>
        /// 등록된 모든 IBloodable 오브젝트의 피를 제거합니다
        /// </summary>
        public void ClearAllBlood()
        {
            foreach (var bloodable in bloodables)
            {
                bloodable.ClearBlood();
            }
        }

        #endregion
    }
}
