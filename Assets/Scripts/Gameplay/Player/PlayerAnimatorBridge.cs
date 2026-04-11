using UnityEngine;

namespace BS.Gameplay.Player
{
    /// <summary>
    /// 玩家动画桥接层。
    /// 只负责把输入和移动状态同步到 Animator，避免和输入、移动强耦合。
    /// </summary>
    public sealed class PlayerAnimatorBridge : MonoBehaviour
    {
        [Header("依赖引用")]
        [SerializeField] private PlayerMotor2D motor;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Animator 参数")]
        [SerializeField] private string moveXParameter = "MoveX";
        [SerializeField] private string moveYParameter = "MoveY";
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private string isMovingParameter = "IsMoving";

        [Header("表现配置")]
        [SerializeField] private bool flipSpriteByFacing = true;

        private void Awake()
        {
            if (motor == null)
            {
                motor = GetComponentInParent<PlayerMotor2D>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (motor == null)
            {
                return;
            }

            var velocity = motor.CurrentVelocity;
            var moveDirection = motor.LastMoveInput;
            var speed = velocity.magnitude;
            var isMoving = speed > 0.01f;

            if (animator != null)
            {
                animator.SetFloat(moveXParameter, moveDirection.x);
                animator.SetFloat(moveYParameter, moveDirection.y);
                animator.SetFloat(speedParameter, speed);
                animator.SetBool(isMovingParameter, isMoving);
            }

            if (flipSpriteByFacing && spriteRenderer != null)
            {
                spriteRenderer.flipX = motor.FacingSign < 0;
            }
        }
    }
}
