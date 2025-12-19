using UnityEngine;

public class Knockbackable : MonoBehaviour
{
    [Header("Collision Settings")]
    public float minSpeedToExplode = 5f;
    public LayerMask wallLayer;

    [Header("Knockback Settings")]
    public float knockbackThreshold = 0.5f;

    private Rigidbody2D rb;
    private bool isKnockBack;

    public bool IsKnockBack => isKnockBack;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isKnockBack)
        {
            if (rb.linearVelocity.magnitude <= knockbackThreshold)
            {
                isKnockBack = false;
            }
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        isKnockBack = true;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        float mySpeed = rb.linearVelocity.magnitude;

        // Wall 레이어와 충돌 시 임계속도 이상이면 폭발
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            if (mySpeed >= minSpeedToExplode)
            {
                Explode();
            }
            return;
        }

        // 다른 Knockbackable과 충돌 시
        Knockbackable knockback = collision.gameObject.GetComponent<Knockbackable>();
        if (knockback == null) return;

        float otherSpeed = knockback.rb.linearVelocity.magnitude;

        if (mySpeed >= minSpeedToExplode || otherSpeed >= minSpeedToExplode)
        {
            knockback.Explode();
            Explode();
        }
    }

    public void Explode()
    {
        Destroy(gameObject);
    }
}
