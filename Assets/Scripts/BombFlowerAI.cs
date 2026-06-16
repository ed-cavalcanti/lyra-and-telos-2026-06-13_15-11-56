using System.Collections;
using UnityEngine;

public class BombFlowerAI : MonoBehaviour
{
    [Header("Detecção & Disparo")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float fireCooldown = 3f;
    [SerializeField] private float delayBetweenShots = 0.25f; // Tempo entre os 2 tiros sequenciais

    [Header("Projétil")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private Animator animator;
    private Transform player;
    private float nextFireTime;
    private bool isFacingRight = true;
    private bool isAwake = false; // Controla se a planta já saiu do modo adormecido

    [SerializeField] private SpriteRenderer spriteRenderer;

    // Parâmetros do Animator (Ajuste conforme os nomes na sua Animation Tree)
    private static readonly int WakeUpHash = Animator.StringToHash("WakeUp");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Start()
    {
        animator = GetComponent<Animator>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // O player entrou no range
        if (distanceToPlayer <= detectionRange)
        {
            if (!isAwake)
            {
                // Acorda a planta
                isAwake = true;
                animator.SetTrigger(WakeUpHash);
                nextFireTime = Time.time + 1f; // Dá 1 segundo para a animação de acordar tocar antes de atirar
            }
            else
            {
                float direction = player.position.x > transform.position.x ? 1f : -1f;
                VerifyFlip(direction);
                TryAttack();
            }
        }
    }

    private void TryAttack()
    {
        if (Time.time >= nextFireTime)
        {
            animator.SetTrigger(AttackHash);
            nextFireTime = Time.time + fireCooldown;
        }
    }

    // ATENÇÃO: Chame este método via Animation Event no frame de ataque da flor!
    public void AnimationEventFireSequential()
    {
        StartCoroutine(FireSequentialRoutine());
    }

    private IEnumerator FireSequentialRoutine()
    {
        for (int i = 0; i < 2; i++)
        {
            FireSingleProjectile();
            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    private void FireSingleProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Vector2 direction = (player.position - firePoint.position).normalized;

        if (proj.TryGetComponent(out BombFlowerProjectile projectileScript))
        {
            AudioManager.Instance.PlaySFX("GargoyleProjectile");
            projectileScript.Initialize(direction);
        }
    }

    private void VerifyFlip(float direction)
    {
        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}