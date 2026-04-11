using UnityEngine;

namespace BS.Gameplay.Player
{
    /// <summary>
    /// 玩家移动执行层。
    /// 负责把输入意图转换成平滑速度，并通过 Rigidbody2D 移动角色。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        [Header("依赖引用")]
        [SerializeField] private PlayerInputReader inputReader;

        [Header("移动参数")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float deceleration = 24f;
        [SerializeField] private float stopThreshold = 0.01f;

        [Header("朝向参数")]
        [SerializeField] private bool faceByHorizontalMovement = true;

        private Rigidbody2D _rigidbody2D;
        private int _movementLockCount;
        private Vector2 _currentVelocity;
        private Vector2 _lastMoveInput = Vector2.right;
        private int _facingSign = 1;

        /// <summary>
        /// 当前物理速度。
        /// </summary>
        public Vector2 CurrentVelocity => _currentVelocity;

        /// <summary>
        /// 最近一次有效输入方向，可供动画层读取。
        /// </summary>
        public Vector2 LastMoveInput => _lastMoveInput;

        /// <summary>
        /// 当前朝向，1 表示朝右，-1 表示朝左。
        /// </summary>
        public int FacingSign => _facingSign;

        /// <summary>
        /// 移动是否可用。
        /// </summary>
        public bool IsMovementEnabled => _movementLockCount <= 0;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();

            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            _rigidbody2D.gravityScale = 0f;
            _rigidbody2D.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            var moveInput = Vector2.zero;
            if (inputReader != null && IsMovementEnabled)
            {
                moveInput = inputReader.MoveInput;
            }

            if (moveInput.sqrMagnitude > 0.0001f)
            {
                _lastMoveInput = moveInput.normalized;

                if (faceByHorizontalMovement && Mathf.Abs(moveInput.x) > 0.01f)
                {
                    _facingSign = moveInput.x > 0f ? 1 : -1;
                }
            }

            var targetVelocity = moveInput * moveSpeed;
            var speedChange = targetVelocity.sqrMagnitude > 0.0001f ? acceleration : deceleration;

            _currentVelocity = Vector2.MoveTowards(
                _rigidbody2D.velocity,
                targetVelocity,
                speedChange * Time.fixedDeltaTime);

            if (_currentVelocity.sqrMagnitude <= stopThreshold * stopThreshold)
            {
                _currentVelocity = Vector2.zero;
            }

            _rigidbody2D.velocity = _currentVelocity;
        }

        /// <summary>
        /// 供交互、过场、对话等系统临时锁住移动。
        /// </summary>
        public void PushMovementLock()
        {
            _movementLockCount++;
            StopImmediately();
        }

        /// <summary>
        /// 对应释放一次移动锁。
        /// </summary>
        public void PopMovementLock()
        {
            _movementLockCount = Mathf.Max(0, _movementLockCount - 1);
        }

        /// <summary>
        /// 直接启用或禁用移动。
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            _movementLockCount = enabled ? 0 : 1;

            if (!enabled)
            {
                StopImmediately();
            }
        }

        /// <summary>
        /// 立即清空速度，防止角色在锁定状态下继续滑动。
        /// </summary>
        public void StopImmediately()
        {
            _currentVelocity = Vector2.zero;
            _rigidbody2D.velocity = Vector2.zero;
        }
    }
}
