using TMPro;
using UnityEngine;

public class EndingText : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI clearTimeText;

    [Header("Format Settings")]
    public string timeFormat = "Clear Time: {0}:{1:00}.{2:00}";

    private void Start()
    {
        DisplayClearTime();
    }

    private void DisplayClearTime()
    {
        float clearTime = GameManager.ClearTime;

        int minutes = Mathf.FloorToInt(clearTime / 60f);
        int seconds = Mathf.FloorToInt(clearTime % 60f);
        int milliseconds = Mathf.FloorToInt((clearTime * 100f) % 100f);

        if (clearTimeText != null)
        {
            clearTimeText.text = string.Format(timeFormat, minutes, seconds, milliseconds);
        }
    }
}
