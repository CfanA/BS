using UnityEngine;

namespace BS.Gameplay.SceneActors
{
    public sealed class SceneActorAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SceneActor actor;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform observedTransform;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Motion")]
        [SerializeField] private float moveThreshold = 0.001f;
        [SerializeField] private bool flipSpriteByFacing = true;

        private Vector3 _lastPosition;
        private bool _hasIsMovingParameter;
        private bool _hasMoveXParameter;
        private bool _hasMoveYParameter;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");

        private void Awake()
        {
            if (actor == null)
            {
                actor = GetComponentInParent<SceneActor>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (observedTransform == null)
            {
                observedTransform = actor != null ? actor.transform : transform;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            _lastPosition = observedTransform != null ? observedTransform.position : transform.position;
            CacheAnimatorParameters();
        }

        private void LateUpdate()
        {
            if (animator == null || observedTransform == null)
            {
                return;
            }

            var currentPosition = observedTransform.position;
            var delta = currentPosition - _lastPosition;
            var isMoving = delta.sqrMagnitude > moveThreshold * moveThreshold;

            if (_hasIsMovingParameter)
            {
                animator.SetBool(IsMovingHash, isMoving);
            }

            if (actor != null)
            {
                var facing = actor.CurrentFacing;
                if (_hasMoveXParameter)
                {
                    animator.SetFloat(MoveXHash, facing.x);
                }

                if (_hasMoveYParameter)
                {
                    animator.SetFloat(MoveYHash, facing.y);
                }

                if (flipSpriteByFacing && spriteRenderer != null && Mathf.Abs(facing.x) > 0.01f)
                {
                    spriteRenderer.flipX = facing.x < 0f;
                }
            }

            _lastPosition = currentPosition;
        }

        private void CacheAnimatorParameters()
        {
            if (animator == null)
            {
                return;
            }

            var parameters = animator.parameters;
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                switch (parameter.name)
                {
                    case "IsMoving":
                        _hasIsMovingParameter = parameter.type == AnimatorControllerParameterType.Bool;
                        break;

                    case "MoveX":
                        _hasMoveXParameter = parameter.type == AnimatorControllerParameterType.Float;
                        break;

                    case "MoveY":
                        _hasMoveYParameter = parameter.type == AnimatorControllerParameterType.Float;
                        break;
                }
            }
        }
    }
}
