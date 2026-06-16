using UnityEngine;

public class MandrakeAI : MonoBehaviour
{
    [Header("Comportamento Mandrágora")]
    [SerializeField] private bool startBuried = true;
    private bool isBuried;
    private bool isEmerging;
    private bool isAttacking; // Impede que ela ande enquanto ataca

    [Header("Movimentação & Patrulha")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Detecção & Ataque")]
    [SerializeField] private float chaseRange = 7f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackOffsetX = 0.8f;
    [SerializeField] private float attackOffsetY = 0f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Referências")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D hitCollider; // Opcional: para desativar a hitbox enquanto enterrada

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private float nextAttackTime;
    private bool isFacingRight = true; // Ajustado para falso se o sprite original olhar para a esquerda

    // Parâmetros do Animator
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int EmergeHash = Animator.StringToHash("Emerge");

    private Vector2 AttackPoint
    {
        get
        {
            float dir = isFacingRight ? 1f : -1f;
            return (Vector2)transform.position + new Vector2(attackOffsetX * dir, attackOffsetY);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        isBuried = startBuried;

        // Se quiser que ela seja invulnerável enquanto enterrada:
        if (isBuried && hitCollider != null) hitCollider.enabled = false;

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

        // 1. ESTADO: Enterrado
        if (isBuried)
        {
            if (distanceToPlayer <= chaseRange) StartEmerging();
            return;
        }

        // 2. ESTADO: Saindo da terra
        if (isEmerging)
        {
            StopMoving();
            return;
        }

        // 3. ESTADO: Atacando (NOVA ADIÇÃO AQUI)
        if (isAttacking)
        {
            StopMoving(); // Garante que o atrito/movimento seja zero
            return;       // Ignora a perseguição até a animação acabar
        }

        // 4. ESTADO: Ativo (Decide se vai perseguir ou iniciar ataque)
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
        }
    }

    private void StartEmerging()
    {
        isBuried = false;
        isEmerging = true;
        animator.SetTrigger(EmergeHash);
    }

    // --- ANIMATION EVENTS ---

    // Chame este método no ÚLTIMO FRAME da animação "Emerging Mid-way" / "Standing Idle"
    public void AnimationEventFinishEmerging()
    {
        isEmerging = false;

        // Reativa a hitbox ao sair da terra
        if (hitCollider != null) hitCollider.enabled = true;

        Debug.Log("[Mandrágora] Saiu da terra! Pronta para atacar.");
    }

    public void AnimationEventAttackDamage()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(AttackPoint, attackRadius, playerLayer);
        foreach (Collider2D col in hitPlayers)
        {
            if (col.TryGetComponent(out PlayerHealth health)) // Supondo que você tenha um PlayerHealth
            {
                health.TakeDamage(1, transform);
            }
        }
    }

    // --- LÓGICA DE MOVIMENTO (Mantida do original) ---

    private void ChasePlayer()
    {
        // Calcula a diferença real no eixo X
        float xDifference = player.position.x - transform.position.x;
        float direction = 0f;

        // Se a distância no eixo X for maior que 0.2f (Deadzone), ele decide o lado.
        // Se for menor, ele mantém a direção atual para não bugar quando o player estiver em cima.
        if (Mathf.Abs(xDifference) > 0.2f)
        {
            direction = xDifference > 0 ? 1f : -1f;
        }
        else
        {
            direction = isFacingRight ? 1f : -1f;
        }

        bool isGroundedAhead = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (isGroundedAhead)
        {
            // Move usando a direção calculada
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
            AudioManager.Instance.PlaySFX("MandrakeAttack");
            float xDifference = player.position.x - transform.position.x;
            float direction = isFacingRight ? 1f : -1f;

            if (Mathf.Abs(xDifference) > 0.2f)
            {
                direction = xDifference > 0 ? 1f : -1f;
            }
            VerifyFlip(direction);

            isAttacking = true; // <-- Trava a IA no estado de ataque
            animator.SetTrigger(AttackHash);
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    public void AnimationEventFinishAttack()
    {
        isAttacking = false;
    }

    private bool IsPlayerOnSamePlatform()
    {
        return Mathf.Abs(transform.position.y - player.position.y) < 2f;
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

        // Espelha o objeto inteiro no eixo X (incluindo a imagem e os colliders filhos)
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        float dir = isFacingRight ? 1f : -1f;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(attackOffsetX * dir, attackOffsetY);
        Gizmos.DrawWireSphere(attackPos, attackRadius);
    }
}