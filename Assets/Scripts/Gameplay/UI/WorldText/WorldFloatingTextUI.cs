using BS.Core;
using UnityEngine;

namespace BS.Gameplay.UI.WorldText
{
    /// <summary>
    /// 世界锚点浮动文本管理器。
    /// 在屏幕 UI 上生成跟随世界对象的文本实例。
    /// </summary>
    public sealed class WorldFloatingTextUI : MonoBehaviour
    {
        [Header("依赖引用")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private RectTransform container;
        [SerializeField] private WorldFloatingTextEntry entryPrefab;

        [Header("默认配置")]
        [SerializeField] private Vector3 defaultWorldOffset = new Vector3(0f, 1.4f, 0f);
        [SerializeField] private Vector2 defaultScreenOffset = Vector2.zero;
        [SerializeField] private float defaultFadeInDuration = 0.2f;
        [SerializeField] private float defaultVisibleDuration = 1.5f;
        [SerializeField] private float defaultFadeOutDuration = 0.3f;
        [SerializeField] private float defaultFloatDistance = 24f;

        public static WorldFloatingTextUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                return;
            }

            Instance = this;
            TryBindCamera();
        }

        private void OnEnable()
        {
            TryBindCamera();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public WorldFloatingTextEntry ShowText(Transform target, string message)
        {
            return ShowText(
                target,
                message,
                defaultWorldOffset,
                defaultScreenOffset,
                defaultFadeInDuration,
                defaultVisibleDuration,
                defaultFadeOutDuration,
                defaultFloatDistance);
        }

        public WorldFloatingTextEntry ShowText(
            Transform target,
            string message,
            Vector3 worldOffset,
            Vector2 screenOffset,
            float fadeInDuration,
            float visibleDuration,
            float fadeOutDuration,
            float floatDistance)
        {
            if (target == null || entryPrefab == null || container == null)
            {
                return null;
            }

            TryBindCamera();
            if (worldCamera == null)
            {
                return null;
            }

            var entry = Instantiate(entryPrefab, container);
            entry.Play(
                container,
                worldCamera,
                target,
                message,
                worldOffset,
                screenOffset,
                fadeInDuration,
                visibleDuration,
                fadeOutDuration,
                floatDistance);

            return entry;
        }

        private void TryBindCamera()
        {
            if (worldCamera != null)
            {
                return;
            }

            if (Camera.main != null)
            {
                worldCamera = Camera.main;
                return;
            }

            if (GameManager.Instance != null)
            {
                worldCamera = FindFirstObjectByType<Camera>(FindObjectsInactive.Exclude);
            }
        }
    }
}
