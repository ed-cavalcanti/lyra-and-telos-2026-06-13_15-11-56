using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Configurações do Projétil")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    [Header("Efeitos")]
    [SerializeField] private GameObject hitEffectPrefab;

    private Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Garante que o tiro suma depois de um tempo se não bater em nada
        Destroy(gameObject, lifetime);
    }

    // Método chamado pelo olho do Boss para dar a direção inicial
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        rb.linearVelocity = direction * speed;

        // Rotaciona o sprite para apontar na direção do movimento
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignora colisão com o próprio boss, com os olhos ou outros inimigos
        if (other.CompareTag("Boss")) return;

        // Verifica se acertou o jogador
        if (other.CompareTag("Player"))
        {
            // AQUI: Substitua 'PlayerHealth' pelo nome real do script que gerencia a vida do seu jogador
            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.TakeDamage(damage);
            }

            Debug.Log("[EnemyProjectile] Acertou o jogador!");
            SpawnHitEffect();
            Destroy(gameObject);
        }
        // Se bater no chão/paredes, o tiro também é destruído
        else if (other.CompareTag("Ground"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}