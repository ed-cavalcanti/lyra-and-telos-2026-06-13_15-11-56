using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BombFlowerHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Morte Explosiva")]
    [SerializeField] private float preExplosionDelay = 0.6f; // Tempo piscando antes de explodir
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private int explosionDamage = 2;
    [SerializeField] private float flashSpeed = 0.08f; // Velocidade do pisca-pisca
    [SerializeField] private GameObject explosionParticles; // Prefab de partículas (opcional)

    [Header("Flash ao tomar dano comum")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float damageFlashDuration = 0.1f;

    public UnityEvent OnDeath;

    private Color originalColor;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int amount, Transform damageSource = null)
    {
        if (isDead) return; // Evita tomar dano enquanto já está explodindo

        currentHealth -= amount;

        if (currentHealth > 0)
        {
            StartCoroutine(DamageFlash());
        }
        else
        {
            StartCoroutine(ExplosiveDeathRoutine());
        }
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    private IEnumerator ExplosiveDeathRoutine()
    {
        isDead = true;
        OnDeath?.Invoke();

        // 1. Desativa IA e Colisão para ela parar de atirar e o player poder atravessar
        if (TryGetComponent(out BombFlowerAI ai)) ai.enabled = false;
        if (TryGetComponent(out Collider2D col)) col.enabled = false;
        if (TryGetComponent(out Animator anim)) anim.enabled = false;

        // 2. Pisca em vermelho rapidamente (indicando a bomba prestes a estourar)
        float elapsedTime = 0f;
        bool isRed = false;

        while (elapsedTime < preExplosionDelay)
        {
            spriteRenderer.color = isRed ? originalColor : damageColor;
            isRed = !isRed;
            yield return new WaitForSeconds(flashSpeed);
            elapsedTime += flashSpeed;
        }

        // 3. Executa a explosão
        Explode();
    }

    private void Explode()
    {
        // Cria efeito visual se houver
        if (explosionParticles != null)
        {
            Instantiate(explosionParticles, transform.position, Quaternion.identity);
        }

        // Causa dano em área usando física 2D
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hitObjects)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent(out PlayerHealth playerHealth))
            {
                // Passa a própria posição como origem do dano para calcular knockback
                playerHealth.TakeDamage(explosionDamage, transform);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha a área da explosão no Editor para facilitar o level design
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}