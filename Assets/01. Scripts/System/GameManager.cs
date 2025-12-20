using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI causeOfDeath;
    public GameObject gameoverPanel; // 패널을 켜고 끄기 위해 GameObject로 변경

    private bool isGameOver = false; // 게임 오버 상태 체크

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
    }

    private void Update()
    {
        if (isGameOver && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    // 게임 오버 시 호출할 함수
    public void GameOver(string reason)
    {
        isGameOver = true;
        
        if (causeOfDeath != null)
            causeOfDeath.text = reason;
        
        if (gameoverPanel != null)
            gameoverPanel.SetActive(true);
        
        Time.timeScale = 0f; 
    }

    public void RestartGame()
    {
        // 게임 시간을 다시 흐르게 설정 (정지시켰을 경우)
        Time.timeScale = 1f;
        
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}