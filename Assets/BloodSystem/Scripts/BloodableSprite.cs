using UnityEngine;

namespace BloodSystem
{
    /// <summary>
    /// 개별 스프라이트에 피가 묻을 수 있게 만드는 컴포넌트
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BloodableSprite : MonoBehaviour, IBloodable
    {
        [Header("Settings")]
        [Tooltip("Blood_Sprite 셰이더를 사용하는 머티리얼을 할당하세요")]
        [SerializeField] private Material bloodMaterial;

        private SpriteRenderer spriteRenderer;
        private RenderTexture bloodMaskRT;
        private MaterialPropertyBlock propertyBlock;
        private Sprite currentSprite;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            propertyBlock = new MaterialPropertyBlock();

            // 머티리얼 설정
            if (bloodMaterial != null)
            {
                spriteRenderer.sharedMaterial = bloodMaterial;
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
            // 스프라이트가 변경되었는지 확인
            if (spriteRenderer.sprite != currentSprite)
            {
                InitializeBloodMask();
            }
        }

        private void InitializeBloodMask()
        {
            if (spriteRenderer.sprite == null)
                return;

            currentSprite = spriteRenderer.sprite;

            // 기존 RT 정리
            CleanupRenderTexture();

            // 스프라이트 픽셀 크기 가져오기
            Rect textureRect = currentSprite.textureRect;
            int width = Mathf.RoundToInt(textureRect.width);
            int height = Mathf.RoundToInt(textureRect.height);

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
            if (bloodMaskRT == null || currentSprite == null)
                return;

            // PropertyBlock 설정
            spriteRenderer.GetPropertyBlock(propertyBlock);

            // BloodMask 설정
            propertyBlock.SetTexture("_BloodMask", bloodMaskRT);

            // Atlas UV 정규화를 위한 Min/Max 계산
            Texture2D mainTexture = currentSprite.texture;
            Rect textureRect = currentSprite.textureRect;

            Vector2 uvMin = new Vector2(
                textureRect.xMin / mainTexture.width,
                textureRect.yMin / mainTexture.height
            );

            Vector2 uvMax = new Vector2(
                textureRect.xMax / mainTexture.width,
                textureRect.yMax / mainTexture.height
            );

            propertyBlock.SetVector("_SpriteUVMin", uvMin);
            propertyBlock.SetVector("_SpriteUVMax", uvMax);

            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        #region IBloodable 구현

        public void AddBlood(Vector2 worldPos, Texture2D splatTexture, float size, float rotation)
        {
            if (bloodMaskRT == null || BloodManager.Instance == null)
                return;

            // 월드 좌표를 로컬 좌표로 변환
            Vector2 localPos = transform.InverseTransformPoint(worldPos);

            // 스프라이트 bounds (로컬 스페이스)
            Bounds spriteBounds = spriteRenderer.sprite.bounds;

            // 로컬 좌표를 UV 좌표로 변환 (0~1)
            Vector2 uv = new Vector2(
                (localPos.x - spriteBounds.min.x) / spriteBounds.size.x,
                (localPos.y - spriteBounds.min.y) / spriteBounds.size.y
            );

            // 월드 크기를 UV 크기로 변환
            float worldToLocalScale = 1f / transform.lossyScale.x; // 스프라이트는 균등 스케일 가정
            float localSize = size * worldToLocalScale;
            Vector2 uvSize = new Vector2(
                localSize / spriteBounds.size.x,
                localSize / spriteBounds.size.y
            );

            // Blit
            BloodManager.Instance.BlitSplat(bloodMaskRT, splatTexture, uv, uvSize, rotation);
        }

        public bool ContainsWorldPoint(Vector2 worldPos)
        {
            if (spriteRenderer.sprite == null)
                return false;

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
            if (spriteRenderer.sprite == null)
                return new Bounds(transform.position, Vector3.zero);

            Bounds localBounds = spriteRenderer.sprite.bounds;
            Bounds worldBounds = new Bounds(
                transform.TransformPoint(localBounds.center),
                Vector3.Scale(localBounds.size, transform.lossyScale)
            );

            return worldBounds;
        }

        #endregion
    }
}
