using UnityEngine;
using UnityEngine.Tilemaps;

namespace BloodSystem
{
    /// <summary>
    /// 타일맵에 피가 묻을 수 있게 만드는 컴포넌트
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    [RequireComponent(typeof(TilemapRenderer))]
    public class BloodableTilemap : MonoBehaviour, IBloodable
    {
        [Header("Settings")]
        [Tooltip("Blood_Tilemap 셰이더를 사용하는 머티리얼을 할당하세요")]
        [SerializeField] private Material bloodMaterial;

        [Tooltip("타일맵 영역 기반 RenderTexture 해상도 (픽셀 per 유닛)")]
        [SerializeField] private float pixelsPerUnit = 16f;

        private Tilemap tilemap;
        private TilemapRenderer tilemapRenderer;
        private RenderTexture bloodMaskRT;
        private MaterialPropertyBlock propertyBlock;
        private Bounds currentBounds;

        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
            tilemapRenderer = GetComponent<TilemapRenderer>();
            propertyBlock = new MaterialPropertyBlock();

            // 머티리얼 설정
            if (bloodMaterial != null)
            {
                tilemapRenderer.sharedMaterial = bloodMaterial;
            }

            InitializeBloodMask();
        }

        private void OnEnable()
        {
            if (BloodManager.Instance != null)
            {
                BloodManager.Instance.RegisterBloodable(this);
            }
        }

        private void OnDisable()
        {
            if (BloodManager.Instance != null)
            {
                BloodManager.Instance.UnregisterBloodable(this);
            }
        }

        private void OnDestroy()
        {
            CleanupRenderTexture();
        }

        private void Update()
        {
            // Bounds가 변경되었는지 확인 (타일 추가/제거 시)
            Bounds newBounds = tilemap.localBounds;
            if (newBounds != currentBounds)
            {
                InitializeBloodMask();
            }
        }

        private void InitializeBloodMask()
        {
            // 타일맵 Bounds 가져오기
            currentBounds = tilemap.localBounds;

            if (currentBounds.size.x <= 0 || currentBounds.size.y <= 0)
            {
                Debug.LogWarning("BloodableTilemap: 타일맵이 비어있거나 유효하지 않은 Bounds를 가지고 있습니다.");
                return;
            }

            // 기존 RT 정리
            CleanupRenderTexture();

            // 월드 Bounds 계산
            Bounds worldBounds = GetWorldBounds();

            // RenderTexture 크기 계산
            int width = Mathf.CeilToInt(worldBounds.size.x * pixelsPerUnit);
            int height = Mathf.CeilToInt(worldBounds.size.y * pixelsPerUnit);

            // 최소/최대 크기 제한
            width = Mathf.Clamp(width, 16, 4096);
            height = Mathf.Clamp(height, 16, 4096);

            // RenderTexture 생성 (R8 포맷)
            bloodMaskRT = new RenderTexture(width, height, 0, RenderTextureFormat.R8);
            bloodMaskRT.filterMode = FilterMode.Bilinear;
            bloodMaskRT.wrapMode = TextureWrapMode.Clamp;

            // 초기화 (검은색 = 피 없음)
            RenderTexture.active = bloodMaskRT;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;

            // PropertyBlock 업데이트
            UpdatePropertyBlock();
        }

        private void CleanupRenderTexture()
        {
            if (bloodMaskRT != null)
            {
                bloodMaskRT.Release();
                Destroy(bloodMaskRT);
                bloodMaskRT = null;
            }
        }

        private void UpdatePropertyBlock()
        {
            if (bloodMaskRT == null)
                return;

            // PropertyBlock 설정
            tilemapRenderer.GetPropertyBlock(propertyBlock);

            // BloodMask 설정
            propertyBlock.SetTexture("_BloodMask", bloodMaskRT);

            // 월드 Bounds 설정
            Bounds worldBounds = GetWorldBounds();
            propertyBlock.SetVector("_WorldBoundsMin", new Vector4(worldBounds.min.x, worldBounds.min.y, 0, 0));
            propertyBlock.SetVector("_WorldBoundsMax", new Vector4(worldBounds.max.x, worldBounds.max.y, 0, 0));

            tilemapRenderer.SetPropertyBlock(propertyBlock);
        }

        #region IBloodable 구현

        public void AddBlood(Vector2 worldPos, Texture2D splatTexture, float size, float rotation)
        {
            if (bloodMaskRT == null || BloodManager.Instance == null)
                return;

            // 월드 좌표를 UV 좌표로 변환
            Bounds worldBounds = GetWorldBounds();
            Vector2 uv = new Vector2(
                (worldPos.x - worldBounds.min.x) / worldBounds.size.x,
                (worldPos.y - worldBounds.min.y) / worldBounds.size.y
            );

            // 월드 크기를 UV 크기로 변환
            Vector2 uvSize = new Vector2(
                size / worldBounds.size.x,
                size / worldBounds.size.y
            );

            // Blit
            BloodManager.Instance.BlitSplat(bloodMaskRT, splatTexture, uv, uvSize, rotation);
        }

        public bool ContainsWorldPoint(Vector2 worldPos)
        {
            Bounds bounds = GetWorldBounds();
            return bounds.Contains(worldPos);
        }

        public RenderTexture GetBloodMaskRT()
        {
            return bloodMaskRT;
        }

        public void ClearBlood()
        {
            if (bloodMaskRT != null)
            {
                RenderTexture.active = bloodMaskRT;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = null;
            }
        }

        public Bounds GetWorldBounds()
        {
            // 로컬 Bounds를 월드 Bounds로 변환
            Bounds localBounds = tilemap.localBounds;
            Bounds worldBounds = new Bounds(
                transform.TransformPoint(localBounds.center),
                Vector3.Scale(localBounds.size, transform.lossyScale)
            );

            return worldBounds;
        }

        #endregion

        #region 디버그

        private void OnDrawGizmosSelected()
        {
            if (tilemap == null)
                tilemap = GetComponent<Tilemap>();

            // Bounds 시각화
            Bounds worldBounds = GetWorldBounds();
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
        }

        #endregion
    }
}
