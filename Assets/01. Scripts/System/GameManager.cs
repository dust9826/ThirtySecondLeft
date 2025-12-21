using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PlayerController player;
    public CinemachineCamera firstcinemachine;
    [Header("UI References")]
    public TextMeshProUGUI causeOfDeath;
    public GameObject gameoverPanel; // 패널을 켜고 끄기 위해 GameObject로 변경
    public GameObject FirstPanel; 

    private bool isGameOver = false; // 게임 오버 상태 체크
    private bool isFirst = false; //시작 연출 시청 여부

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
        
        if(player != null) player.moveSpeed = 0f;
    }

    private void Update()
    {
        // 1. 게임 오버 시 재시작 로직
        if (isGameOver && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }

        // 2. 게임 시작 전(첫 클릭) 로직 추가
        // 아직 시작하지 않았고(isFirst가 false), 마우스 왼쪽 버튼을 눌렀을 때
        if (!isFirst && Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartGameSequence();
        }
    }

    private void StartGameSequence()
    {
        if(FirstPanel && firstcinemachine != null){
        isFirst = true;
        if (FirstPanel != null) FirstPanel.SetActive(false);
    
        player.moveSpeed = 8f;
        if (firstcinemachine != null) firstcinemachine.gameObject.SetActive(true);
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
        gameoverPanel.SetActive(false);
        SceneManager.LoadScene(currentSceneName);
    }
}