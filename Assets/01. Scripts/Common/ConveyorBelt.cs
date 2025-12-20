using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private float conveyorSpeed = 2f;
    [SerializeField] private Vector2 direction = Vector2.left;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.transform.position += (Vector3)(direction.normalized * conveyorSpeed * Time.deltaTime);
        }
    }
}
