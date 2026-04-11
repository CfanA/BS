using UnityEngine;

namespace BS.Gameplay.Player
{
    /// <summary>
    /// 玩家输入读取层。
    /// 只负责把输入转换成移动意图，不直接控制 Rigidbody2D。
    /// </summary>
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [Header("输入配置")]
        [SerializeField] private bool allowVerticalMovement = true;

        private int _inputLockCount;
        private Vector2 _moveInput;

        /// <summary>
        /// 当前移动输入，范围通常在 -1 到 1。
        /// </summary>
        public Vector2 MoveInput => _moveInput;

        /// <summary>
        /// 输入是否可用。
        /// </summary>
        public bool IsInputEnabled => _inputLockCount <= 0;

        private void Update()
        {
            if (!IsInputEnabled)
            {
                _moveInput = Vector2.zero;
                return;
            }

            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = allowVerticalMovement ? Input.GetAxisRaw("Vertical") : 0f;

            _moveInput = new Vector2(horizontal, vertical);
            if (_moveInput.sqrMagnitude > 1f)
            {
                _moveInput.Normalize();
            }
        }

        /// <summary>
        /// 供交互、过场、菜单等系统临时锁住输入。
        /// </summary>
        public void PushInputLock()
        {
            _inputLockCount++;
            _moveInput = Vector2.zero;
        }

        /// <summary>
        /// 对应释放一次输入锁。
        /// </summary>
        public void PopInputLock()
        {
            _inputLockCount = Mathf.Max(0, _inputLockCount - 1);
        }

        /// <summary>
        /// 直接启用或禁用输入，适合简单场景调用。
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            _inputLockCount = enabled ? 0 : 1;

            if (!enabled)
            {
                _moveInput = Vector2.zero;
            }
        }
    }
}
