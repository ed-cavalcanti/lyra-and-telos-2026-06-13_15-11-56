using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;

    [Header("Invencibilidade pós-dano (I-Frames)")]
    [SerializeField] private float invincibleDuration = 0.8f;
    private bool isInvincible = false;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5f;

    [Header("Flash ao tomar dano")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private int flashCount = 3;        // Quantas vezes pisca durante o I-Frame

    public UnityEvent<int, int> OnHealthChanged;
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
        if (isInvincible) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"[Player] Vida: {currentHealth}/{maxHealth}");

        if (rb != null && damageSource != null)
        {
            Vector2 dir = (transform.position - damageSource.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(dir.x, 0.5f) * knockbackForce, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private System.Collections.IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // Pisca N vezes durante a invencibilidade
        int effectiveFlashCount = Mathf.Max(1, flashCount);
        float intervalPerFlash = invincibleDuration / (effectiveFlashCount * 2);
        if (flashDuration > 0f)
            intervalPerFlash = Mathf.Min(flashDuration, intervalPerFlash);

        for (int i = 0; i < effectiveFlashCount; i++)
        {
            SetSpriteColor(damageColor);
            yield return new WaitForSeconds(intervalPerFlash);
            SetSpriteColor(originalColor);
            yield return new WaitForSeconds(intervalPerFlash);
        }

        float remaining = invincibleDuration - (intervalPerFlash * effectiveFlashCount * 2);
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        isInvincible = false;
    }

    private void SetSpriteColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("[Player] Morreu!");

        gameObject.SetActive(false);
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}