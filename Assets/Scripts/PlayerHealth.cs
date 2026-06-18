using UnityEngine;
using UnityEngine.Events;
using TarodevController;

public class PlayerHealth : MonoBehaviour
{
    public static int globalSavedHealth = -1;
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
    [SerializeField] private int flashCount = 3;

    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;

    private Rigidbody2D rb;
    private Color originalColor;

    // === VARIÁVEIS DE CONTROLE ADICIONADAS AQUI ===
    private Vector2 lastCheckpointPosition;
    private bool isDead = false; // Aqui está ela!

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        lastCheckpointPosition = transform.position;

        // Se o jogador veio de outra fase, recupera a vida dele
        if (globalSavedHealth != -1)
        {
            currentHealth = globalSavedHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }

        // A MÁGICA FINAL: Avisa o HUD para desenhar os corações corretos logo no frame 1!
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount, Transform damageSource = null)
    {
        // Trava: Se estiver invencível OU já estiver morto, ignora o dano!
        if (isInvincible || isDead) return;

        currentHealth -= amount;
        globalSavedHealth = currentHealth;
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

    public void SetCheckpoint(Vector2 newPosition)
    {
        lastCheckpointPosition = newPosition;
        Debug.Log("Novo checkpoint salvo em: " + newPosition);
    }

    private System.Collections.IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

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
        isDead = true;

        // Trava o personagem e zera o movimento ao morrer
        TarodevController.PlayerController controller = GetComponent<TarodevController.PlayerController>();
        if (controller != null) controller.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Aciona o Menu de Game Over via Unity Event
        OnDeath?.Invoke();
    }

    public void RespawnAtCheckpoint()
    {
        Heal(maxHealth);
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Teleporta e devolve o controle
        TarodevController.PlayerController controller = GetComponent<TarodevController.PlayerController>();
        if (controller != null)
        {
            controller.TeleportTo(lastCheckpointPosition);
            controller.enabled = true;
        }
        else
        {
            transform.position = lastCheckpointPosition;
        }

        // Corta o Cinemachine (Evita o deslize de câmera)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(lastCheckpointPosition.x, lastCheckpointPosition.y, mainCam.transform.position.z);
            Behaviour cinemachineBrain = (Behaviour)mainCam.GetComponent("CinemachineBrain");
            if (cinemachineBrain != null)
            {
                cinemachineBrain.enabled = false;
                cinemachineBrain.enabled = true;
            }
        }

        isDead = false; // Tira a trava, o jogador está vivo de novo
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        globalSavedHealth = currentHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}