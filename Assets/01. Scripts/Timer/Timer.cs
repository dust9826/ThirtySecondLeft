using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요합니다.

public class Timer : MonoBehaviour
{
    [SerializeField] private float remainingTime = 30f; // 시작 시간 (30초)
    [SerializeField] private TextMeshProUGUI timerText; // UI 텍스트 연결

    void Update()
    {
        if (remainingTime > 5)
        {
            // 매 프레임마다 시간을 차감
            remainingTime -= Time.deltaTime;
            timerText.color = Color.green;
        }
        else if(remainingTime < 5 && remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            timerText.color = Color.red; // 시간이 다 되면 텍스트 색상을 빨간색으로 변경 (선택사항)
        }
        else
        {
            // 0초 이하로 내려가지 않게 고정
            remainingTime = 0;
            GameManager.Instance.GameOver("시간이 모두 지나 보안문이 닫혔습니다!");
            remainingTime = 30f;
        }

        DisplayTime(remainingTime);
    }

    void DisplayTime(float timeToDisplay)
    {
        // 소수점 이하를 버리고 정수형으로 변환
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliseconds = (timeToDisplay % 1) * 100;
        timerText.text = string.Format("{0:00}:{1:00}", seconds, milliseconds);
    }
}