using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TarodevController
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        DialogueSystem dialogueSystem;
        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        private Animator _anim;
        private bool _movementLocked;

        [Header("Combat")]
        [SerializeField] private Transform _attackPoint; // Onde o ataque acontece
        [SerializeField] private float _attackRange = 0.5f; // Tamanho da área de ataque
        [SerializeField] private LayerMask _enemyLayers; // O que é considerado inimigo
        [SerializeField] private int _attackDamage = 10; // Dano do ataque
        [SerializeField] private float _attackRate = 2f; // Quantos ataques por segundo
        private float _nextAttackTime = 0f; // Controle de cooldown

        #region Interface
        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        #endregion

        private float _time;
        private Vector2 _lastSafePosition;
        private float _timeGrounded;

        private void Awake()
        {
            dialogueSystem = FindAnyObjectByType<DialogueSystem>();
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _anim = GetComponent<Animator>();

            if (_anim != null)
            {
                _anim.applyRootMotion = false; // Garante que a animação não trave a física
            }

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            _lastSafePosition = transform.position;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
            UpdateAnimations();

            if (_time >= _nextAttackTime && _frameInput.AttackDown && !_movementLocked)
            {
                Attack();
                _nextAttackTime = _time + 1f / _attackRate; // Calcula o tempo até o próximo ataque permitido
            }
        }

        private void UpdateAnimations()
        {
            if (_anim == null) return;

            // Movimentação Horizontal (Corrida)
            bool isMovingHorizontally = Mathf.Abs(_frameInput.Move.x) > 0.01f;
            _anim.SetBool("isRunning", isMovingHorizontally);

            // ESTES SÃO OS NOVOS PARÂMETROS PARA O PULO:
            _anim.SetBool("isGrounded", _grounded);
            _anim.SetFloat("verticalVelocity", _frameVelocity.y);

            // Inverte a escala horizontal para espelhar o sprite perfeitamente
            if (_frameInput.Move.x > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (_frameInput.Move.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }

        private void Attack()
        {
            // Toca a animação e SÓ ISSO. O dano fica para depois.
            if (_anim != null) _anim.SetTrigger("MeleeAttack");
            AudioManager.Instance.PlaySFX("MeleeAttack");
        }

        // === NOVO: Método para o Animation Event ===
        // Precisa ser "public" (ou visível para a engine) para a Animação conseguir encontrá-lo
        public void ExecuteMeleeDamage()
        {
            if (_attackPoint == null) return;

            // Detecta os inimigos no alcance
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(_attackPoint.position, _attackRange, _enemyLayers);

            // Aplica o dan
            foreach (Collider2D enemy in hitEnemies)
            {
                // 1. Tenta ver se é um inimigo comu
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(_attackDamage, transform);
                    Debug.Log($"[Animation Event] Acertou {enemy.name} e causou {_attackDamage} de dano!");
                }

                // 2. Tenta ver se é a Flor Bomb
                BombFlowerHealth flowerHealth = enemy.GetComponent<BombFlowerHealth>();
                if (flowerHealth != null)
                {
                    flowerHealth.TakeDamage(_attackDamage, transform);
                    Debug.Log($"[Animation Event] Acertou {enemy.name} (Flor) e causou {_attackDamage} de dano!");
                }

                // 3. Tenta ver se é o Chefão (NOVO)
                BossController bossHealth = enemy.GetComponent<BossController>();
                if (bossHealth != null)
                {
                    // Usa a função de dano do boss passando o valor _attackDamag
                    bossHealth.TakeDamage(_attackDamage);
                    Debug.Log($"[Animation Event] Acertou o Chefão e causou {_attackDamage} de dano!");
                }
            }
        }

        public void SetMovementLocked(bool locked)
        {
            if (_movementLocked == locked) return;

            _movementLocked = locked;

            if (locked)
            {
                _frameVelocity = Vector2.zero; // Zera o X e o Y de vez
                _jumpToConsume = false;
                _bufferedJumpUsable = false;
                _timeJumpWasPressed = 0f;

                if (_rb != null)
                {
                    _rb.linearVelocity = Vector2.zero; // Para o corpo físico completamente
                }
            }
        }

        private void GatherInput()
        {
            var keyboard = Keyboard.current;
            var gamepad = Gamepad.current;

            Vector2 moveInput = Vector2.zero;
            bool jumpDownPressed = false;
            bool jumpIsHeld = false;
            bool attackDown = false;

            if (_movementLocked)
            {
                _frameInput = new FrameInput
                {
                    JumpDown = false,
                    JumpHeld = false,
                    Move = Vector2.zero,
                    AttackDown = false
                };
                return;
            }

            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput.x = -1;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput.x = 1;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveInput.y = 1;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveInput.y = -1;

                if (keyboard.spaceKey.wasPressedThisFrame || keyboard.cKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
                {
                    jumpDownPressed = true;
                }
                if (keyboard.spaceKey.isPressed || keyboard.cKey.isPressed || keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                    jumpIsHeld = true;
                if (keyboard.jKey.wasPressedThisFrame) attackDown = true;
            }

            if (gamepad != null && moveInput == Vector2.zero)
            {
                moveInput = gamepad.leftStick.ReadValue();
                if (gamepad.buttonSouth.wasPressedThisFrame) jumpDownPressed = true;
                if (gamepad.buttonSouth.isPressed) jumpIsHeld = true;
                if (gamepad.buttonWest.wasPressedThisFrame) attackDown = true;
            }

            _frameInput = new FrameInput
            {
                JumpDown = jumpDownPressed,
                JumpHeld = jumpIsHeld,
                Move = moveInput,
                AttackDown = attackDown
            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        private void FixedUpdate()
        {
            CheckCollisions();
            UpdateSafePosition();

            HandleJump();
            HandleDirection();
            HandleGravity();

            ApplyMovement();
        }

        #region Collisions
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            var bounds = _col.bounds;
            var probeSize = new Vector2(bounds.size.x * 0.9f, _stats.GrounderDistance);
            var groundProbePos = new Vector2(bounds.center.x, bounds.min.y - probeSize.y * 0.5f);
            var ceilingProbePos = new Vector2(bounds.center.x, bounds.max.y + probeSize.y * 0.5f);

            // 1. Criamos uma máscara que ignora a layer "OneWayPlatform" para o teto
            int ceilingMask = ~_stats.PlayerLayer;
            int oneWayLayer = LayerMask.NameToLayer("OneWayPlatform");
            if (oneWayLayer != -1)
            {
                ceilingMask &= ~(1 << oneWayLayer); // Remove a plataforma do detector de teto
            }

            // 2. O teto agora usa a nova máscara blindada
            bool ceilingHit = Physics2D.OverlapBox(ceilingProbePos, probeSize, 0f, ceilingMask);
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // 3. O chão SÓ é detectado se o personagem estiver caindo ou parado 
            // (Isso evita bugar e pisar na plataforma enquanto ainda está atravessando ela para cima)
            bool groundHit = false;
            if (_frameVelocity.y <= 0f)
            {
                // --- INÍCIO DA MODIFICAÇÃO PARA IGNORAR O ESPINHO ---
                bool originalQueriesHitTriggers = Physics2D.queriesHitTriggers;
                Physics2D.queriesHitTriggers = false;

                groundHit = Physics2D.OverlapBox(groundProbePos, probeSize, 0f, ~_stats.PlayerLayer);

                Physics2D.queriesHitTriggers = originalQueriesHitTriggers;
            }

            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                // _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }
        }
        #endregion

        #region Jumping
        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }
        #endregion

        #region Horizontal
        private void HandleDirection()
        {
            if (_movementLocked)
            {
                _frameVelocity.x = 0f;
                return;
            }

            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }
        #endregion

        #region Gravity
        private void HandleGravity()
        {
            // ADICIONE ESTA TRAVA AQUI:
            if (_movementLocked)
            {
                _frameVelocity.y = 0f;
                return;
            }

            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }
        #endregion

        #region SafePostion
        private void UpdateSafePosition()
        {
            // Só contabiliza se estiver no chão e não estiver pulando
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _timeGrounded += Time.fixedDeltaTime;

                // Exige 0.25 segundos pisando no chão para considerar "seguro"
                if (_timeGrounded >= 0.3f)
                {
                    // Salva a posição, mas um pouquinho mais alta (Vector3.up * 0.2f)
                    // Isso evita que o personagem spawne "dentro" do chão ou escorregue
                    _lastSafePosition = transform.position + (Vector3.up * 0.2f);

                    // O GRANDE SEGREDO: Zera o tempo depois de salvar!
                    // Isso faz com que o script só salve uma nova posição se você 
                    // passar MAIS 0.25 segundos no chão. Mantendo o save sempre longe da beirada.
                    _timeGrounded = 0f;
                }
            }
            else
            {
                // Se saiu do chão (pulou ou caiu), zera o contador
                _timeGrounded = 0f;
            }
        }

        // Método para o sistema de respawn chamar
        public void TeleportTo(Vector2 position)
        {
            // 1. Zera todas as velocidades acumuladas
            _frameVelocity = Vector2.zero;

            // 2. Move a Transform visual
            transform.position = position;

            if (_rb != null)
            {
                // 3. Move o componente físico e zera a inércia
                _rb.position = position;
                _rb.linearVelocity = Vector2.zero;
            }

            // 4. ESTA É A LINHA MÁGICA: Força a Unity a atualizar tudo imediatamente, 
            // ignorando qualquer cálculo pendente de gravidade ou atraso de interpolação.
            Physics2D.SyncTransforms();
        }

        public Vector2 GetSafePosition() => _lastSafePosition;
        #endregion

        private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

        private void OnDrawGizmosSelected()
        {
            if (_attackPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_attackPoint.position, _attackRange);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;
        public bool AttackDown;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}