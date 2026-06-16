using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // NOVO: Necessário para trocar de cena

[RequireComponent(typeof(BossStarRain))]
public class BossController : MonoBehaviour
{
    [Header("Atributos de Vida")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Animação")]
    [SerializeField] private Animator anim;

    [Header("Configurações da IA")]
    public float timeBetweenAttacks = 3f;
    private float attackTimer;

    [Header("Configurações de Vulnerabilidade")]
    public float vulnerableDuration = 4f;
    private float vulnerableTimer;

    [Header("Configurações de Teleporte")]
    [SerializeField] private Transform[] teleportPoints;
    [SerializeField] private float teleportCooldown = 5f;
    [SerializeField] private int hitsToTeleport = 4;
    private float teleportTimer;
    private int currentHitCount;
    private int lastPointIndex = -1;

    [Header("Efeitos de Dano")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    private Color originalColor;

    [Header("Efeitos de Morte Dramática")]
    [SerializeField] private float deathSequenceDuration = 3f;
    [SerializeField] private float deathFlashSpeed = 0.1f;
    [SerializeField] private Color deathFlashColor = Color.white;
    [SerializeField] private GameObject deathExplosionPrefab;
    [SerializeField] private float timeBetweenExplosions = 0.15f;

    [Header("Transição Pós-Morte")]
    [Tooltip("Nome exato da cena a ser carregada após o boss explodir")]
    [SerializeField] private string nextSceneName; // NOVO: Define para onde ir após a vitória

    public enum BossState { Idle, Attacking, Vulnerable, Dead }
    public BossState currentState;

    private BossStarRain starRainSkill;

    void Start()
    {
        currentHealth = maxHealth;
        starRainSkill = GetComponent<BossStarRain>();
        currentState = BossState.Idle;

        attackTimer = timeBetweenAttacks;
        teleportTimer = teleportCooldown;
        currentHitCount = 0;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (anim == null)
            anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (currentState == BossState.Dead) return;

        if (currentState == BossState.Idle)
        {
            attackTimer -= Time.deltaTime;
            teleportTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                StartCoroutine(PerformAttack());
            }
            else if (teleportTimer <= 0f)
            {
                TeleportToRandomPoint();
            }
        }
        else if (currentState == BossState.Vulnerable)
        {
            vulnerableTimer -= Time.deltaTime;

            if (vulnerableTimer <= 0f)
            {
                currentState = BossState.Idle;
                if (anim != null) anim.SetBool("IsVulnerable", false);

                attackTimer = timeBetweenAttacks;
                teleportTimer = teleportCooldown;
                currentHitCount = 0;
            }
        }
    }

    private IEnumerator PerformAttack()
    {
        currentState = BossState.Attacking;

        yield return StartCoroutine(starRainSkill.CastStarRain());

        TeleportToRandomPoint();
        currentState = BossState.Vulnerable;
        vulnerableTimer = vulnerableDuration;

        if (anim != null) anim.SetBool("IsVulnerable", true);
    }

    public void TakeDamage(int damage)
    {
        if (currentState == BossState.Dead) return;

        currentHealth -= damage;
        Debug.Log($"Chefão tomou {damage} de dano! Vida: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(DamageFlash());

        if (currentState != BossState.Vulnerable)
        {
            currentHitCount++;
            if (currentHitCount >= hitsToTeleport)
            {
                TeleportToRandomPoint();
            }
        }
    }

    private void TeleportToRandomPoint()
    {
        if (teleportPoints == null || teleportPoints.Length == 0) return;

        int randomIndex = Random.Range(0, teleportPoints.Length);
        if (teleportPoints.Length > 1)
        {
            while (randomIndex == lastPointIndex) randomIndex = Random.Range(0, teleportPoints.Length);
        }

        lastPointIndex = randomIndex;
        transform.position = teleportPoints[randomIndex].position;

        teleportTimer = teleportCooldown;
        currentHitCount = 0;
    }

    private IEnumerator DamageFlash()
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
        currentState = BossState.Dead;
        StopAllCoroutines();

        if (anim != null)
        {
            anim.SetBool("IsVulnerable", false);
            anim.Play("Idle");
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DramaticDeathRoutine());
    }

    private IEnumerator DramaticDeathRoutine()
    {
        float elapsedTime = 0f;
        float nextExplosionTime = 0f;
        bool isWhite = false;
        Bounds bounds = spriteRenderer.bounds;

        // Loop das explosões dramáticas
        while (elapsedTime < deathSequenceDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isWhite ? originalColor : deathFlashColor;
                isWhite = !isWhite;
            }

            if (elapsedTime >= nextExplosionTime && deathExplosionPrefab != null)
            {
                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomY = Random.Range(bounds.min.y, bounds.max.y);
                Vector3 explosionPos = new Vector3(randomX, randomY, transform.position.z);

                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("FlowerExplode");
                Instantiate(deathExplosionPrefab, explosionPos, Quaternion.identity);

                nextExplosionTime = elapsedTime + timeBetweenExplosions;
            }

            yield return new WaitForSeconds(deathFlashSpeed);
            elapsedTime += deathFlashSpeed;
        }

        // --- NOVO: Lógica da Transição Pós-Morte ---

        // Esconde o sprite do boss para dar a sensação de que ele foi destruído
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        TransitionManager tm = TransitionManager.Instance;
        if (tm == null) tm = FindAnyObjectByType<TransitionManager>();

        if (tm != null && !string.IsNullOrEmpty(nextSceneName))
        {
            // Aciona o fade out e carrega a próxima cena quando a tela ficar totalmente preta
            tm.DoTransition(() =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        }
        else
        {
            // Fallback de segurança: Se esquecer de configurar, apenas destrói o Boss
            Debug.LogWarning("[BossController] TransitionManager não encontrado ou NextSceneName vazio! Apenas destruindo o objeto.");
            Destroy(gameObject);
        }
    }
}