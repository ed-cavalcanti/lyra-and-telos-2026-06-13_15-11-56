using UnityEngine;

public class GargoyleAI : MonoBehaviour
{
    [Header("Detecção & Disparo")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float fireCooldown = 3f;

    [Header("Projétil")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint; // Ponto de onde o tiro sai (ex: no peito/mãos)

    // Componentes e referências
    private Animator animator;
    private Transform player;
    private float nextFireTime;
    private bool isFacingRight = true;

    [SerializeField] private SpriteRenderer spriteRenderer;

    // Parâmetros do Animator
    private static readonly int AttackHash = Animator.StringToHash("Attack"); // Gatilho de carregar/atirar

    private void Start()
    {
        animator = GetComponent<Animator>();

        // Busca o jogador pela Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Se o player entrar no range da gárgula
        if (distanceToPlayer <= detectionRange)
        {
            // Define a direção horizontal (-1 para esquerda, 1 para direita) e vira a Gárgula
            float direction = player.position.x > transform.position.x ? 1f : -1f;
            VerifyFlip(direction);

            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time >= nextFireTime)
        {
            // Dispara a animação (idle -> charge -> shoot)
            animator.SetTrigger(AttackHash);

            // Define o próximo tiro com base no cooldown
            nextFireTime = Time.time + fireCooldown;
        }
    }

    // ATENÇÃO: Chame este método via Animation Event no frame exato em que ela "solta" a magia!
    public void AnimationEventFireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null)
        {
            Debug.LogWarning("Faltam referências para atirar o projétil!");
            return;
        }

        // Instancia o projétil na posição do FirePoint
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Calcula a direção do peito da gárgula até o centro do jogador
        Vector2 direction = (player.position - firePoint.position).normalized;

        // Inicializa o tiro passando a direção
        if (proj.TryGetComponent(out GargoyleProjectile projectileScript))
        {
            projectileScript.Initialize(direction);
        }
    }

    private void VerifyFlip(float direction)
    {
        if (direction > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (direction < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
}