using BS.Core;
using BS.Foundation.Ids;
using BS.Gameplay.Story.Actions;
using BS.Gameplay.Story.Conditions;
using UnityEngine;

namespace BS.Gameplay.Story
{
    /// <summary>
    /// 条件式剧情事件执行器。
    /// 条件满足时执行一组动作，并支持只触发一次。
    /// </summary>
    public sealed class ConditionalStoryEvent : MonoBehaviour
    {
        [Header("触发配置")]
        [SerializeField] private bool evaluateOnStart = true;
        [SerializeField] private bool evaluateOnFlagChanged = true;
        [SerializeField] private bool triggerOnlyOnce = true;
        [SerializeField] private FlagReference triggeredRecordFlag;

        [Header("条件")]
        [SerializeField] private FlagConditionGroup conditions = new();

        [Header("动作")]
        [SerializeField] private StoryAction[] actions;

        private bool _hasTriggeredThisSession;

        private void OnEnable()
        {
            if (GameManager.Instance != null && GameManager.Instance.Flags != null && evaluateOnFlagChanged)
            {
                GameManager.Instance.Flags.FlagChanged += HandleFlagChanged;
            }
        }

        private void Start()
        {
            if (evaluateOnStart)
            {
                TryExecute();
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null && GameManager.Instance.Flags != null && evaluateOnFlagChanged)
            {
                GameManager.Instance.Flags.FlagChanged -= HandleFlagChanged;
            }
        }

        public bool TryExecute()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Flags == null)
            {
                return false;
            }

            if (triggerOnlyOnce && HasTriggered(gameManager.Flags))
            {
                return false;
            }

            if (!conditions.Evaluate(gameManager.Flags))
            {
                return false;
            }

            if (actions != null)
            {
                for (var i = 0; i < actions.Length; i++)
                {
                    actions[i]?.Execute(gameManager);
                }
            }

            MarkTriggered(gameManager.Flags);
            return true;
        }

        private void HandleFlagChanged(FlagId flagId, bool value)
        {
            TryExecute();
        }

        private bool HasTriggered(FlagManager flagManager)
        {
            if (_hasTriggeredThisSession)
            {
                return true;
            }

            return triggeredRecordFlag.IsValid && flagManager.GetFlag(triggeredRecordFlag.FlagId);
        }

        private void MarkTriggered(FlagManager flagManager)
        {
            _hasTriggeredThisSession = true;

            if (triggeredRecordFlag.IsValid)
            {
                flagManager.SetFlag(triggeredRecordFlag.FlagId, true);
            }
        }
    }
}
