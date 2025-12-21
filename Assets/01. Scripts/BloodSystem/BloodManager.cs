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
        [SerializeField] private GameObject[] splatPrefabs;

        [SerializeField] private Transform splatParents;

        [SerializeField] Color bloodColorMin = Color.red;
        [SerializeField] Color bloodColorMax = Color.red;

        [Header("Object Pooling")]
        [Tooltip("최대 스플래터 개수. 이 수를 초과하면 가장 오래된 스플래터를 재활용합니다.")]
        [SerializeField] private int maxSplatCount = 100;

        private Queue<GameObject> splatPool = new Queue<GameObject>();
        
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
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
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
            // 회전이 지정되지 않았으면 랜덤
            if (rotation < 0)
            {
                rotation = Random.Range(0f, Mathf.PI * 2f);
            }

            // 스플래터 텍스처 선택
            GameObject splatPrefab = GetSplatPrefabs(splatIndex);
            if (splatPrefab == null)
            {
                Debug.LogWarning("BloodManager: 유효한 스플래터 텍스처를 찾을 수 없습니다!");
                return;
            }

            GameObject newSplat;

            // 풀이 최대 크기에 도달했으면 가장 오래된 스플래터 재활용
            if (splatPool.Count >= maxSplatCount)
            {
                newSplat = splatPool.Dequeue();

                // null 체크 (씬 전환 등으로 파괴되었을 수 있음)
                if (newSplat == null)
                {
                    newSplat = Instantiate(splatPrefab, worldPos, Quaternion.Euler(0, 0, rotation), splatParents);
                }
                else
                {
                    // 위치와 회전 재설정
                    newSplat.transform.position = worldPos;
                    newSplat.transform.rotation = Quaternion.Euler(0, 0, rotation);
                }
            }
            else
            {
                // 새로 생성
                newSplat = Instantiate(splatPrefab, worldPos, Quaternion.Euler(0, 0, rotation), splatParents);
            }

            // 풀에 추가 (큐 끝에 추가되어 가장 최신으로 표시됨)
            splatPool.Enqueue(newSplat);
        }

        #endregion

        #region 프리팹 유틸리티

        /// <summary>
        /// 스플래터 텍스처를 가져옵니다
        /// </summary>
        /// <param name="index">인덱스. -1이면 랜덤</param>
        /// <returns>스플래터 텍스처</returns>
        public GameObject GetSplatPrefabs(int index = -1)
        {
            if (splatPrefabs == null || splatPrefabs.Length == 0)
                return null;

            if (index < 0 || index >= splatPrefabs.Length)
            {
                // 랜덤 선택
                index = Random.Range(0, splatPrefabs.Length);
            }

            return splatPrefabs[index];
        }

        /// <summary>
        /// 랜덤 스플래터 텍스처를 가져옵니다
        /// </summary>
        public GameObject GetRandomSplatTexture()
        {
            return GetSplatPrefabs(-1);
        }
        public Color RandomColorRGB(Color min, Color max, float alpha = 1f)
        {
            return new Color(
                Random.Range(min.r, max.r),
                Random.Range(min.g, max.g),
                Random.Range(min.b, max.b),
                alpha
            );
        }

        #endregion

    }
}
