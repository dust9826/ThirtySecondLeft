using BloodSystem;
using UnityEngine;

public class Knockbackable : MonoBehaviour
{
    [Header("Collision Settings")]
    public float minSpeedToExplode = 5f;
    public LayerMask wallLayer;

    [Header("Knockback Settings")]
    public float knockbackThreshold = 0.5f;

    [Header("Crash Audio")] 
    public AudioClip crashClip;

    public float crashSensity;
    private AudioSource playerAudio;
    
    private Rigidbody2D rb;
    private bool isKnockBack;

    public bool IsKnockBack => isKnockBack;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        playerAudio = GameObject.FindWithTag("Player")
            .GetComponent<AudioSource>();
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
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
        else
        {
            rb.linearVelocity = direction * force;
        }
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
                Explode(collision);
            }
            return;
        }

        // 다른 Knockbackable과 충돌 시
        Knockbackable knockback = collision.gameObject.GetComponent<Knockbackable>();
        if (knockback == null) return;

        float otherSpeed = knockback.rb.linearVelocity.magnitude;

        if (mySpeed >= minSpeedToExplode || otherSpeed >= minSpeedToExplode)
        {
            knockback.Explode(collision);
            Explode(collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag.Equals("Bullet"))
        {
            float mySpeed = rb.linearVelocity.magnitude;

            // 다른 Knockbackable과 충돌 시
            Knockbackable knockback = other.gameObject.GetComponent<Knockbackable>();
            if (knockback == null) return;
            if (!knockback.isKnockBack) return;
            
            float otherSpeed = knockback.rb.linearVelocity.magnitude;

            if (mySpeed >= minSpeedToExplode || otherSpeed >= minSpeedToExplode)
            {
                knockback.Explode(other);
                Explode(other);
            }
        }
    }

    public void Explode(Collision2D collision)
    {
        BloodEmitter emitter = GetComponent<BloodEmitter>();
        if (emitter != null)
        {
            // 방법 1: Collision2D 직접 전달
            emitter.EmitFromCollision(collision);
            GoreSpawner.Instance.Spawn(collision);
            playerAudio.PlayOneShot(crashClip);
            playerAudio.volume = crashSensity;
        }

        Destroy(gameObject);
    }

    public void Explode(Collider2D collider)
    {
        BloodEmitter emitter = GetComponent<BloodEmitter>();
        if (emitter != null)
        {
            // 방법 1: Collision2D 직접 전달
            emitter.Emit(this.transform.position, Vector2.zero);
            GoreSpawner.Instance.Spawn(transform.position, Vector2.zero);
            playerAudio.PlayOneShot(crashClip);
            playerAudio.volume = crashSensity;
        }

        Destroy(gameObject);
    }
}
