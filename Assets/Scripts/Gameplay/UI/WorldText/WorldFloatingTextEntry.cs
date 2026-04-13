using System.Collections;
using TMPro;
using UnityEngine;

namespace BS.Gameplay.UI.WorldText
{
    /// <summary>
    /// 单条浮动文本实例，负责跟随世界锚点并播放淡入淡出。
    /// </summary>
    public sealed class WorldFloatingTextEntry : MonoBehaviour
    {
        [Header("UI 引用")]
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private CanvasGroup canvasGroup;

        private Camera _worldCamera;
        private RectTransform _container;
        private RectTransform _rectTransform;
        private Canvas _containerCanvas;
        private Transform _target;
        private Vector3 _worldOffset;
        private Vector2 _screenOffset;
        private float _floatDistance;
        private Coroutine _playRoutine;
        private bool _isBehindCamera;
        private float _currentAlpha;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void LateUpdate()
        {
            if (_target == null || _rectTransform == null || _container == null || _worldCamera == null)
            {
                return;
            }

            UpdateAnchoredPosition(0f);
        }

        public void Play(
            RectTransform container,
            Camera worldCamera,
            Transform target,
            string message,
            Vector3 worldOffset,
            Vector2 screenOffset,
            float fadeInDuration,
            float visibleDuration,
            float fadeOutDuration,
            float floatDistance)
        {
            _container = container;
            _worldCamera = worldCamera;
            _containerCanvas = container != null ? container.GetComponentInParent<Canvas>() : null;
            _target = target;
            _worldOffset = worldOffset;
            _screenOffset = screenOffset;
            _floatDistance = floatDistance;
            _isBehindCamera = false;

            if (messageText != null)
            {
                messageText.text = message ?? string.Empty;
            }

            if (canvasGroup != null)
            {
                _currentAlpha = 0f;
                ApplyCanvasAlpha();
            }

            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
            }

            UpdateAnchoredPosition(0f);
            _playRoutine = StartCoroutine(PlayRoutine(fadeInDuration, visibleDuration, fadeOutDuration));
        }

        private IEnumerator PlayRoutine(float fadeInDuration, float visibleDuration, float fadeOutDuration)
        {
            yield return FadeTo(1f, Mathf.Max(0f, fadeInDuration));
            yield return Hold(Mathf.Max(0f, visibleDuration));
            yield return FadeTo(0f, Mathf.Max(0f, fadeOutDuration));
            Destroy(gameObject);
        }

        private IEnumerator Hold(float duration)
        {
            if (duration <= 0f)
            {
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                UpdateAnchoredPosition(progress);
                yield return null;
            }
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                _currentAlpha = targetAlpha;
                ApplyCanvasAlpha();
                yield break;
            }

            var startAlpha = _currentAlpha;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                _currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                ApplyCanvasAlpha();
                UpdateAnchoredPosition(progress);
                yield return null;
            }

            _currentAlpha = targetAlpha;
            ApplyCanvasAlpha();
        }

        private void UpdateAnchoredPosition(float animationProgress)
        {
            var worldPosition = _target.position + _worldOffset;
            var screenPoint = _worldCamera.WorldToScreenPoint(worldPosition);

            if (screenPoint.z < 0f)
            {
                _isBehindCamera = true;
                ApplyCanvasAlpha();
                return;
            }

            if (_isBehindCamera)
            {
                _isBehindCamera = false;
                ApplyCanvasAlpha();
            }

            var floatingOffset = new Vector2(0f, _floatDistance * animationProgress);
            var targetScreenPoint = (Vector2)screenPoint + _screenOffset + floatingOffset;
            var uiCamera = _containerCanvas != null && _containerCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _containerCanvas.worldCamera
                : null;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _container,
                targetScreenPoint,
                uiCamera,
                out var localPoint);

            _rectTransform.anchoredPosition = localPoint;
        }

        private void ApplyCanvasAlpha()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = _isBehindCamera ? 0f : _currentAlpha;
            }
        }
    }
}
