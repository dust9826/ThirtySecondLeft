using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.Cinemachine;

public class FirstWarp : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Timer Settings")]
    [SerializeField] private Transform secondElevator;
    [SerializeField] private float timerDuration = 60f;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private Transform[] leftSpawnPositions = new Transform[4];
    [SerializeField] private Transform[] rightSpawnPositions = new Transform[4];
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 10f;

    [Header("Camera Shake Settings")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float shakeIntensity = 2f;
    [SerializeField] private float shakeDuration = 0.5f;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI floorText;

    private bool isWarping = false;
    private Transform playerTransform;
    private int spawnCount = 0;
    private CinemachineBasicMultiChannelPerlin noise;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !isWarping)
        {
            playerTransform = other.transform;
            StartCoroutine(WarpWithFade(other.transform));
        }
    }

    private IEnumerator WarpWithFade(Transform player)
    {
        isWarping = true;

        // Fade Out
        yield return StartCoroutine(Fade(0f, 1f));

        // Warp
        player.position = new Vector3(102f, -2f, 0f);

        // Fade In
        yield return StartCoroutine(Fade(1f, 0f));

        isWarping = false;

        // 2초 후 타이머 시작
        yield return new WaitForSeconds(2f);
        StartCoroutine(TimerRoutine());
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        yield return new WaitForSeconds(timerDuration);

        // 60초 후 SecondElevator로 이동
        yield return StartCoroutine(WarpToSecondElevator());
    }

    private IEnumerator SpawnRoutine()
    {
        float elapsed = 0f;

        while (elapsed < timerDuration)
        {
            // 스폰 및 층수 표시
            spawnCount++;
            SpawnEnemies();
            UpdateFloorText();

            yield return new WaitForSeconds(spawnInterval - 1f);
            elapsed += spawnInterval;
        }
    }

    private void SpawnEnemies()
    {
        int spawnPerSide;

        if (spawnCount >= 5)
            spawnPerSide = 4; // 8명 (좌4, 우4)
        else if (spawnCount >= 3)
            spawnPerSide = 3; // 6명 (좌3, 우3)
        else
            spawnPerSide = 2; // 4명 (좌2, 우2)

        for (int i = 0; i < spawnPerSide; i++)
        {
            if (i < leftSpawnPositions.Length && leftSpawnPositions[i] != null)
                Instantiate(enemyPrefab, leftSpawnPositions[i].position, Quaternion.identity);

            if (i < rightSpawnPositions.Length && rightSpawnPositions[i] != null)
                Instantiate(enemyPrefab, rightSpawnPositions[i].position, Quaternion.identity);
        }
    }
    
    private void UpdateFloorText()
    {
        floorText.text = spawnCount.ToString("0") + "F";
    }

    private IEnumerator WarpToSecondElevator()
    {
        isWarping = true;

        // Fade Out
        yield return StartCoroutine(Fade(0f, 1f));

        // Warp to SecondElevator
        playerTransform.position = secondElevator.position;

        // Fade In
        yield return StartCoroutine(Fade(1f, 0f));

        isWarping = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }
}
