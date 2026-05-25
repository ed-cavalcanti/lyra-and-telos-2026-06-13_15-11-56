using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 4f;

    [Header("Flash ao tomar dano")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    public UnityEvent OnDeath;

    private Rigidbody2D rb;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int amount, Transform damageSource = null)
    {
        currentHealth -= amount;
        Debug.Log($"[Inimigo] Vida: {currentHealth}/{maxHealth}");

        // Knockback
        if (rb != null && damageSource != null)
        {
            Vector2 dir = (transform.position - damageSource.position).normalized;
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0) Die();
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("[Inimigo] Morreu!");
        Destroy(gameObject);
    }
}