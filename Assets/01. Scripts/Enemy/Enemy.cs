using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Collision Settings")]
    public float minSpeedToExplode = 5f;  // 이 속도 이상일 때 충돌하면 터짐

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 상대가 Enemy인지 확인
        Enemy otherEnemy = collision.gameObject.GetComponent<Enemy>();
        if (otherEnemy == null) return;

        // 내 속도 또는 상대 속도가 일정 이상이면 둘 다 터짐
        float mySpeed = rb.linearVelocity.magnitude;
        float otherSpeed = otherEnemy.rb.linearVelocity.magnitude;

        if (mySpeed >= minSpeedToExplode || otherSpeed >= minSpeedToExplode)
        {
            otherEnemy.Explode();
            Explode();
        }
    }

    public void Explode()
    {
        // TODO: 폭발 이펙트, 사운드 등 추가 가능
        Destroy(gameObject);
    }
}
