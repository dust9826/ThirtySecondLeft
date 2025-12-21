using UnityEngine;

public class EndTimer : MonoBehaviour
{
    void Awake()
    {
        GameManager.Instance.StopTimer();
    }
}
