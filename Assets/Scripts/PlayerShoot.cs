using UnityEngine;
using UnityEngine.InputSystem;
using TarodevController;

public class PlayerShoot : MonoBehaviour
{
    [Header("Tiro")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float chargeTime = 0.6f;     // Deve bater com a duração da animação
    [SerializeField] private Key attackKey = Key.Z;

    [Header("Direção")]
    [SerializeField] private SpriteRenderer playerSprite;

    private Animator animator;
    private PlayerController playerController;
    private float holdTimer = 0f;
    private bool isCharging = false;
    private bool hasShot = false;           // Evita atirar mais de uma vez por carga

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private bool hasAttackParam = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.nameHash == AttackHash) hasAttackParam = true;
    }

    private void Update()
    {
        // Começou a segurar
        if (WasAttackPressedThisFrame() && !isCharging)
        {
            isCharging = true;
            hasShot = false;
            holdTimer = 0f;

            playerController?.SetMovementLocked(true);

            if (hasAttackParam)
                animator.SetTrigger(AttackHash);
        }

        // Segurando — acumula o tempo
        if (isCharging && IsAttackHeld())
        {
            holdTimer += Time.deltaTime;

            // Tempo completo → dispara
            if (holdTimer >= chargeTime && !hasShot)
            {
                Shoot();
                hasShot = true;
                playerController?.SetMovementLocked(false);
            }
        }

        // Soltou a tecla antes de completar — cancela sem atirar
        if (isCharging && WasAttackReleasedThisFrame())
        {
            isCharging = false;

            if (!hasShot)
            {
                // Interrompe a animação voltando para Idle
                animator.ResetTrigger(AttackHash);
                animator.Play("Idle");
                Debug.Log("[PlayerShoot] Carga cancelada.");
            }

            playerController?.SetMovementLocked(false);
        }

        // Reseta o estado após o tiro
        if (hasShot && WasAttackReleasedThisFrame())
        {
            isCharging = false;
        }
    }

    private void Shoot()
    {
        Vector2 direction = GetFacingDirection();

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        if (proj.TryGetComponent(out Projectile projectile))
        {
            projectile.SetDirection(direction);
        }

        Debug.Log("[PlayerShoot] Projétil disparado!");
    }

    private Vector2 GetFacingDirection()
    {
        // Considera flipX e escala negativa para evitar tiro invertido.
        if (playerSprite == null)
        {
            float fallbackSign = Mathf.Sign(transform.lossyScale.x);
            if (Mathf.Approximately(fallbackSign, 0f)) fallbackSign = 1f;
            return fallbackSign < 0f ? Vector2.left : Vector2.right;
        }

        float scaleSign = Mathf.Sign(playerSprite.transform.lossyScale.x);
        if (Mathf.Approximately(scaleSign, 0f)) scaleSign = 1f;
        float flipSign = playerSprite.flipX ? -1f : 1f;
        float facing = scaleSign * flipSign;

        return facing < 0f ? Vector2.left : Vector2.right;
    }

    private bool WasAttackPressedThisFrame()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return false;

        var keyControl = keyboard[attackKey];
        return keyControl != null && keyControl.wasPressedThisFrame;
    }

    private bool WasAttackReleasedThisFrame()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return false;

        var keyControl = keyboard[attackKey];
        return keyControl != null && keyControl.wasReleasedThisFrame;
    }

    private bool IsAttackHeld()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return false;

        var keyControl = keyboard[attackKey];
        return keyControl != null && keyControl.isPressed;
    }
}