using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private bool useNextScene = true;
    [SerializeField] private string targetSceneName;
    [SerializeField] private int targetSceneIndex;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (useNextScene)
            {
                SceneMove.Instance.LoadNextScene();
            }
            else if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneMove.Instance.LoadScene(targetSceneName);
            }
            else
            {
                SceneMove.Instance.LoadScene(targetSceneIndex);
            }
        }
    }
}
