using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projétil")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;       // Destrói se não acertar nada

    [Header("Efeito ao acertar (opcional)")]
    [SerializeField] private GameObject hitEffectPrefab; // Deixe vazio por enquanto

    private Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Chamado pelo PlayerShoot logo após instanciar
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        rb.linearVelocity = direction * speed;

        // Rotaciona o sprite para apontar na direção do tiro
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignora colisão com o próprio player
        if (other.CompareTag("Player")) return;

        // Acertou um inimigo?
        if (other.TryGetComponent(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(damage);
        }

        SpawnHitEffect();
        Destroy(gameObject);
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}