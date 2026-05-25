using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TarodevController
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        private Animator _anim;
        private bool _movementLocked;

        #region Interface
        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _anim = GetComponent<Animator>();

            if (_anim != null)
            {
                _anim.applyRootMotion = false; // Garante que a animação não trave a física
            }

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
            UpdateAnimations();
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

        public void SetMovementLocked(bool locked)
        {
            if (_movementLocked == locked) return;

            _movementLocked = locked;

            if (locked)
            {
                _frameVelocity.x = 0f;
                _jumpToConsume = false;
                _bufferedJumpUsable = false;
                _timeJumpWasPressed = 0f;

                if (_rb != null)
                {
                    _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
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

            if (_movementLocked)
            {
                _frameInput = new FrameInput
                {
                    JumpDown = false,
                    JumpHeld = false,
                    Move = Vector2.zero
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
            }

            if (gamepad != null && moveInput == Vector2.zero)
            {
                moveInput = gamepad.leftStick.ReadValue();
                if (gamepad.buttonSouth.wasPressedThisFrame) jumpDownPressed = true;
                if (gamepad.buttonSouth.isPressed) jumpIsHeld = true;
            }

            _frameInput = new FrameInput
            {
                JumpDown = jumpDownPressed,
                JumpHeld = jumpIsHeld,
                Move = moveInput
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

            bool groundHit = Physics2D.OverlapBox(groundProbePos, probeSize, 0f, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.OverlapBox(ceilingProbePos, probeSize, 0f, ~_stats.PlayerLayer);

            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

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

        private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

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
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}