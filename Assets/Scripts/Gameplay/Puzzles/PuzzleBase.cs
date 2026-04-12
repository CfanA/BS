using System;
using BS.Core;
using BS.Gameplay.Story.Conditions;
using UnityEngine;

namespace BS.Gameplay.Puzzles
{
    /// <summary>
    /// 通用谜题基类。
    /// 负责状态管理、条件激活、解开后的结果执行。
    /// 具体输入和判题逻辑由子类实现。
    /// </summary>
    public abstract class PuzzleBase : MonoBehaviour
    {
        [Header("存档配置")]
        [SerializeField] private string puzzleId;

        [Header("状态配置")]
        [SerializeField] private bool evaluateAvailabilityOnStart = true;
        [SerializeField] private bool evaluateAvailabilityOnFlagChanged = true;
        [SerializeField] private FlagConditionGroup availabilityConditions = new();

        [Header("解题结果")]
        [SerializeField] private PuzzleResultSet solvedResults = new();

        public PuzzleState State { get; private set; } = PuzzleState.Inactive;
        public bool IsSolved => State == PuzzleState.Solved;
        public bool IsAvailable => State == PuzzleState.Available;

        public event Action<PuzzleState> StateChanged;
        public event Action Solved;

        public bool TryGetSaveId(out string id)
        {
            id = puzzleId;
            return !string.IsNullOrWhiteSpace(id);
        }

        protected virtual void OnEnable()
        {
            if (evaluateAvailabilityOnFlagChanged && GameManager.Instance != null && GameManager.Instance.Flags != null)
            {
                GameManager.Instance.Flags.FlagChanged += HandleFlagChanged;
            }
        }

        protected virtual void Start()
        {
            if (evaluateAvailabilityOnStart)
            {
                RefreshAvailability();
            }
        }

        protected virtual void OnDisable()
        {
            if (evaluateAvailabilityOnFlagChanged && GameManager.Instance != null && GameManager.Instance.Flags != null)
            {
                GameManager.Instance.Flags.FlagChanged -= HandleFlagChanged;
            }
        }

        public void RefreshAvailability()
        {
            if (IsSolved)
            {
                return;
            }

            var nextState = EvaluateCanActivate() ? PuzzleState.Available : PuzzleState.Inactive;
            SetState(nextState);
        }

        public bool TryActivate()
        {
            if (IsSolved)
            {
                return false;
            }

            RefreshAvailability();
            return IsAvailable;
        }

        protected bool SolvePuzzle()
        {
            if (IsSolved)
            {
                return false;
            }

            SetState(PuzzleState.Solved);
            solvedResults.Execute(GameManager.Instance);
            Solved?.Invoke();
            return true;
        }

        public void RestoreSolvedStateFromSave()
        {
            if (IsSolved)
            {
                return;
            }

            SetState(PuzzleState.Solved);
            OnRestoredAsSolved();
        }

        public bool DebugMarkSolved()
        {
            var solved = SolvePuzzle();
            if (solved)
            {
                Debug.Log($"[DebugPanel] 谜题已标记为完成: {name}", this);
            }

            return solved;
        }

        public void DebugResetToInactive()
        {
            SetState(PuzzleState.Inactive);
            RefreshAvailability();
            Debug.Log($"[DebugPanel] 谜题已重置: {name}", this);
        }

        public void DebugRefreshAvailability()
        {
            RefreshAvailability();
            Debug.Log($"[DebugPanel] 谜题可用性已刷新: {name} -> {State}", this);
        }

        protected void SetState(PuzzleState nextState)
        {
            if (State == nextState)
            {
                return;
            }

            State = nextState;
            StateChanged?.Invoke(State);
        }

        protected virtual bool EvaluateCanActivate()
        {
            var flagManager = GameManager.Instance != null ? GameManager.Instance.Flags : null;
            return flagManager == null || availabilityConditions.Evaluate(flagManager);
        }

        protected virtual void OnRestoredAsSolved()
        {
        }

        private void HandleFlagChanged(Foundation.Ids.FlagId flagId, bool value)
        {
            RefreshAvailability();
        }
    }
}
