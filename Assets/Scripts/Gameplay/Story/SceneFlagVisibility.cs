using BS.Core;
using BS.Foundation.Ids;
using UnityEngine;

namespace BS.Gameplay.Story
{
    /// <summary>
    /// 根据剧情 Flag 控制场景物体显示隐藏。
    /// 适合“调查后拿到物品/触发剧情后，场景中的对应物件消失”的场景。
    /// </summary>
    public sealed class SceneFlagVisibility : MonoBehaviour
    {
        [Header("Flag 配置")]
        [SerializeField] private string flagId;
        [SerializeField] private bool expectedValue = true;

        [Header("显示配置")]
        [SerializeField] private GameObject targetObject;
        [SerializeField] private bool hideWhenMatched = true;
        [SerializeField] private bool refreshOnStart = true;

        [Header("依赖引用")]
        [SerializeField] private FlagManager flagManager;

        private bool _isSubscribed;

        private void Reset()
        {
            if (targetObject == null)
            {
                targetObject = gameObject;
            }
        }

        private void Awake()
        {
            TryBindFlagManager();
        }

        private void OnEnable()
        {
            TryBindFlagManager();

            if (flagManager != null && !_isSubscribed)
            {
                flagManager.FlagChanged += HandleFlagChanged;
                _isSubscribed = true;
            }
        }

        private void Start()
        {
            if (refreshOnStart)
            {
                RefreshVisibility();
            }
        }

        private void Update()
        {
            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (flagManager != null && _isSubscribed)
            {
                flagManager.FlagChanged -= HandleFlagChanged;
                _isSubscribed = false;
            }
        }

        [ContextMenu("Refresh Visibility")]
        public void RefreshVisibility()
        {
            if (targetObject == null)
            {
                return;
            }

            TryBindFlagManager();
            if (flagManager == null || string.IsNullOrWhiteSpace(flagId))
            {
                return;
            }

            var currentValue = flagManager.GetFlag(new FlagId(flagId));
            var isMatched = currentValue == expectedValue;
            var shouldShow = hideWhenMatched ? !isMatched : isMatched;
            targetObject.SetActive(shouldShow);
        }

        private void HandleFlagChanged(FlagId changedFlagId, bool value)
        {
            if (!string.Equals(changedFlagId.Value?.Trim(), flagId?.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            RefreshVisibility();
        }

        private void TryBindFlagManager()
        {
            if (flagManager == null && GameManager.Instance != null)
            {
                flagManager = GameManager.Instance.Flags;
            }
        }
    }
}
