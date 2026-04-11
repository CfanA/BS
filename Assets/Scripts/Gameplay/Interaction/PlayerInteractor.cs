using System;
using System.Collections.Generic;
using BS.Gameplay.Player;
using UnityEngine;

namespace BS.Gameplay.Interaction
{
    /// <summary>
    /// 玩家唯一的对外交互入口。
    /// 负责检测范围内可交互对象、选择当前目标、响应交互按键，并向 UI 广播提示变化。
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [Header("依赖引用")]
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerMotor2D motor;

        [Header("交互配置")]
        [SerializeField] private LayerMask interactableLayers = ~0;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private bool allowInteractWithoutInput = true;

        private readonly Dictionary<IInteractable, int> _candidateRefCounts = new();
        private int _interactionLockCount;
        private IInteractable _currentTarget;

        public event Action<IInteractable> CurrentInteractableChanged;
        public event Action<string, string> PromptShown;
        public event Action PromptHidden;

        public IInteractable CurrentTarget => _currentTarget;
        public bool IsInteractionEnabled => _interactionLockCount <= 0;

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (motor == null)
            {
                motor = GetComponent<PlayerMotor2D>();
            }
        }

        private void Update()
        {
            CleanupCandidates();
            RefreshCurrentTarget();

            if (!IsInteractionEnabled)
            {
                return;
            }

            if (!allowInteractWithoutInput && inputReader != null && !inputReader.IsInputEnabled)
            {
                return;
            }

            if (_currentTarget != null && Input.GetKeyDown(interactKey))
            {
                _currentTarget.Interact(this);
                RefreshCurrentTarget();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryRegisterInteractable(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TryUnregisterInteractable(other);
        }

        public void PushInteractionLock()
        {
            _interactionLockCount++;
        }

        public void PopInteractionLock()
        {
            _interactionLockCount = Mathf.Max(0, _interactionLockCount - 1);
        }

        public void SetInteractionEnabled(bool enabled)
        {
            _interactionLockCount = enabled ? 0 : 1;
        }

        private void TryRegisterInteractable(Collider2D other)
        {
            if (!IsLayerIncluded(other.gameObject.layer))
            {
                return;
            }

            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null)
            {
                return;
            }

            if (_candidateRefCounts.TryGetValue(interactable, out var count))
            {
                _candidateRefCounts[interactable] = count + 1;
                return;
            }

            _candidateRefCounts[interactable] = 1;
            interactable.OnEnterRange(this);
            RefreshCurrentTarget();
        }

        private void TryUnregisterInteractable(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null)
            {
                return;
            }

            if (!_candidateRefCounts.TryGetValue(interactable, out var count))
            {
                return;
            }

            if (count > 1)
            {
                _candidateRefCounts[interactable] = count - 1;
                return;
            }

            if (_candidateRefCounts.Remove(interactable))
            {
                interactable.OnExitRange(this);
                RefreshCurrentTarget();
            }
        }

        private void CleanupCandidates()
        {
            var invalidCandidates = ListPool<IInteractable>.Get();

            foreach (var pair in _candidateRefCounts)
            {
                var candidate = pair.Key;
                if (candidate is not UnityEngine.Object unityObject || unityObject == null)
                {
                    invalidCandidates.Add(candidate);
                }
            }

            for (var i = 0; i < invalidCandidates.Count; i++)
            {
                _candidateRefCounts.Remove(invalidCandidates[i]);
            }

            ListPool<IInteractable>.Release(invalidCandidates);
        }

        private void RefreshCurrentTarget()
        {
            IInteractable bestTarget = null;
            var bestScore = float.MinValue;

            foreach (var pair in _candidateRefCounts)
            {
                var candidate = pair.Key;
                if (candidate == null || !candidate.CanInteract(this))
                {
                    continue;
                }

                var score = CalculateCandidateScore(candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = candidate;
                }
            }

            if (ReferenceEquals(_currentTarget, bestTarget))
            {
                UpdatePrompt(bestTarget);
                return;
            }

            _currentTarget = bestTarget;
            CurrentInteractableChanged?.Invoke(_currentTarget);
            UpdatePrompt(_currentTarget);
        }

        private float CalculateCandidateScore(IInteractable candidate)
        {
            var interactionPoint = candidate.GetInteractionPoint();
            var origin = (Vector2)transform.position;
            var toTarget = interactionPoint - origin;
            var distance = toTarget.magnitude;

            var facingDirection = Vector2.right;
            if (motor != null && motor.LastMoveInput.sqrMagnitude > 0.0001f)
            {
                facingDirection = motor.LastMoveInput.normalized;
            }

            var directionScore = distance > 0.001f
                ? Mathf.Max(0f, Vector2.Dot(facingDirection, toTarget.normalized))
                : 1f;

            return candidate.Priority * 1000f + directionScore * 100f - distance;
        }

        private void UpdatePrompt(IInteractable interactable)
        {
            if (interactable != null && interactable.CanInteract(this))
            {
                PromptShown?.Invoke(interactable.PromptText, interactable.DisplayName);
                return;
            }

            PromptHidden?.Invoke();
        }

        private bool IsLayerIncluded(int layer)
        {
            return (interactableLayers.value & (1 << layer)) != 0;
        }

        /// <summary>
        /// 简单列表池，避免频繁临时分配。
        /// 当前只在交互检测内部使用，保持实现轻量。
        /// </summary>
        private static class ListPool<T>
        {
            private static readonly Stack<List<T>> Pool = new();

            public static List<T> Get()
            {
                return Pool.Count > 0 ? Pool.Pop() : new List<T>();
            }

            public static void Release(List<T> list)
            {
                list.Clear();
                Pool.Push(list);
            }
        }
    }
}
