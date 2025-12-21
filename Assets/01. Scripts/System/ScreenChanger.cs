using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필수

public class SceneChanger : MonoBehaviour
{
    // 이동할 씬의 이름을 인스펙터 창에서 직접 입력할 수 있습니다.
    [SerializeField] private string nextSceneName;

    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}