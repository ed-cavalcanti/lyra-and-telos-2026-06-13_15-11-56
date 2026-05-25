using UnityEngine;

public class MeleeEnemyAI : MonoBehaviour
{
    [Header("Movimentação & Patrulha")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Detecção & Ataque")]
    [SerializeField] private float chaseRange = 7f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackOffsetX = 0.8f; // Distância à frente do inimigo
    [SerializeField] private float attackOffsetY = 0f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask playerLayer;

    // Componentes e referências
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private float nextAttackTime;
    private bool isFacingRight = true; // Ajuste baseado na direção padrão do seu sprite

    // Parâmetros do Animator (Hashes para performance)
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Vector2 AttackPoint
    {
        get
        {
            float dir = isFacingRight ? 1f : -1f;
            return (Vector2)transform.position + new Vector2(attackOffsetX * dir, attackOffsetY);
        }
    }

    [SerializeField] private SpriteRenderer spriteRenderer;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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

        // Máquina de estados baseada na distância
        if (distanceToPlayer <= attackRange)
        {
            StopMoving();
            TryAttack();
        }
        else if (distanceToPlayer <= chaseRange && IsPlayerOnSamePlatform())
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
            // Aqui você pode chamar uma função de Patrulha se desejar
        }
    }

    private void ChasePlayer()
    {
        // Define a direção horizontal (-1 para esquerda, 1 para direita)
        float direction = player.position.x > transform.position.x ? 1f : -1f;

        // Verifica se há chão à frente antes de mover (evita suicídio de plataformas)
        bool isGroundedAhead = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (isGroundedAhead)
        {
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
            animator.SetFloat(SpeedHash, Mathf.Abs(rb.linearVelocity.x));
            VerifyFlip(direction);
        }
        else
        {
            StopMoving();
        }
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetFloat(SpeedHash, 0f);
    }

    private void TryAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            // Olha para o jogador antes de desferir o golpe
            float direction = player.position.x > transform.position.x ? 1f : -1f;
            VerifyFlip(direction);

            // Dispara a animação
            animator.SetTrigger(AttackHash);
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    // Chamado via Animation Event no frame exato do impacto do ataque
    // Substitua o método existente por este:
    public void AnimationEventAttackDamage()
    {
        Debug.Log($"[Ataque] AttackPoint calculado: {AttackPoint}");

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(AttackPoint, attackRadius);

        Debug.Log($"[Ataque] Colliders encontrados: {hitPlayers.Length}");

        foreach (Collider2D col in hitPlayers)
        {
            Debug.Log($"[Ataque] Acertou: {col.gameObject.name} | Layer: {LayerMask.LayerToName(col.gameObject.layer)}");

            if (col.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(1, transform);
            }
        }
    }

    private bool IsPlayerOnSamePlatform()
    {
        // Altura limite (Y) para o inimigo decidir se vale a pena perseguir
        return Mathf.Abs(transform.position.y - player.position.y) < 2f;
    }

    private void VerifyFlip(float direction)
    {
        // Se o sprite padrão olha para a esquerda (como na imagem original)
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

    // Desenha os limites no editor para fácil configuração
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Gizmo do AttackPoint agora usa a propriedade dinâmica
        Gizmos.color = Color.magenta;
        float dir = isFacingRight ? 1f : -1f;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffsetX * dir, attackOffsetY);
        Gizmos.DrawWireSphere(attackPos, attackRadius);
    }
}