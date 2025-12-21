using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Target Layers")]
    [SerializeField] private LayerMask targetLayers;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsInTargetLayer(other.gameObject))
        {
            Destroy(other.gameObject);
        }
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayers.value & (1 << obj.layer)) != 0;
    }
}
