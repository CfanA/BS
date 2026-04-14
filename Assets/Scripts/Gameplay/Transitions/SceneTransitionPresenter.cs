using System.Collections;
using BS.Gameplay.Transitions.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BS.Gameplay.Transitions
{
    /// <summary>
    /// 全局常驻的过场展示器。
    /// 负责持有黑幕、白幕、剧情介绍文字和可选标题文本。
    /// </summary>
    public sealed class SceneTransitionPresenter : MonoBehaviour
    {
        [Header("生命周期")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        [Header("配置")]
        [SerializeField] private SceneIntroDatabase introDatabase;

        [Header("UI 引用")]
        [SerializeField] private GameObject root;
        [SerializeField] private Image blackOverlayImage;
        [SerializeField] private Image whiteOverlayImage;
        [SerializeField] private CanvasGroup introTextGroup;
        [SerializeField] private TMP_Text introText;
        [SerializeField] private CanvasGroup titleGroup;
        [SerializeField] private TMP_Text titleText;

        public static SceneTransitionPresenter Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(transform.root.gameObject);
            }

            ResetState();
            SetRootVisible(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool TryGetProfile(string sceneName, out SceneIntroProfile profile)
        {
            if (introDatabase != null)
            {
                return introDatabase.TryGetProfile(sceneName, out profile);
            }

            profile = null;
            return false;
        }

        public void SetRootVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }

        public void ResetState()
        {
            SetBlackAlpha(0f);
            SetWhiteAlpha(0f);
            ClearIntroText();
            ClearTitle();
        }

        public void CoverWithBlack()
        {
            SetRootVisible(true);
            SetBlackAlpha(1f);
            SetWhiteAlpha(0f);
            ClearIntroText();
            ClearTitle();
        }

        public IEnumerator FadeBlackTo(float targetAlpha, float duration)
        {
            if (blackOverlayImage == null)
            {
                yield break;
            }

            yield return FadeImageTo(blackOverlayImage, targetAlpha, duration);
        }

        public IEnumerator FadeWhiteTo(float targetAlpha, float duration)
        {
            if (whiteOverlayImage == null)
            {
                yield break;
            }

            yield return FadeImageTo(whiteOverlayImage, targetAlpha, duration);
        }

        public IEnumerator ShowIntroText(SceneIntroProfile profile)
        {
            if (profile == null
                || profile.TextDisplayMode == SceneIntroTextDisplayMode.None
                || string.IsNullOrWhiteSpace(profile.IntroText)
                || introText == null
                || introTextGroup == null)
            {
                Debug.LogWarning("[SceneTransitionPresenter] 跳过介绍文字显示，配置为空或引用缺失。", this);
                yield break;
            }

            Debug.LogWarning($"[SceneTransitionPresenter] 显示介绍文字: {profile.IntroText}", this);
            introTextGroup.alpha = 0f;

            if (profile.EnableTextFadeIn)
            {
                yield return FadeCanvasGroupTo(introTextGroup, 1f, profile.TextFadeInDuration);
            }
            else
            {
                introTextGroup.alpha = 1f;
            }

            switch (profile.TextDisplayMode)
            {
                case SceneIntroTextDisplayMode.Instant:
                    introText.text = profile.IntroText;
                    break;

                case SceneIntroTextDisplayMode.Typewriter:
                    yield return PlayTypewriter(profile.IntroText, profile.TypewriterCharactersPerSecond);
                    break;
            }

            if (profile.TextHoldDuration > 0f)
            {
                yield return new WaitForSeconds(profile.TextHoldDuration);
            }

            if (profile.EnableTextFadeOut)
            {
                yield return FadeCanvasGroupTo(introTextGroup, 0f, profile.TextFadeOutDuration);
            }

            ClearIntroText();
        }

        public IEnumerator ShowTitle(SceneIntroProfile profile)
        {
            if (profile == null
                || string.IsNullOrWhiteSpace(profile.TitleText)
                || titleGroup == null
                || titleText == null)
            {
                yield break;
            }

            titleText.text = profile.TitleText;
            titleGroup.alpha = 0f;

            if (profile.EnableTitleFadeIn)
            {
                yield return FadeCanvasGroupTo(titleGroup, 1f, profile.TitleFadeInDuration);
            }
            else
            {
                titleGroup.alpha = 1f;
            }

            if (profile.TitleHoldDuration > 0f)
            {
                yield return new WaitForSeconds(profile.TitleHoldDuration);
            }

            if (profile.EnableTitleFadeOut)
            {
                yield return FadeCanvasGroupTo(titleGroup, 0f, profile.TitleFadeOutDuration);
            }

            ClearTitle();
        }

        private IEnumerator PlayTypewriter(string content, float charactersPerSecond)
        {
            introText.text = string.Empty;

            if (charactersPerSecond <= 0f)
            {
                introText.text = content;
                yield break;
            }

            var interval = 1f / charactersPerSecond;
            for (var i = 1; i <= content.Length; i++)
            {
                introText.text = content.Substring(0, i);
                if (i < content.Length)
                {
                    yield return new WaitForSeconds(interval);
                }
            }
        }

        private IEnumerator FadeImageTo(Image image, float targetAlpha, float duration)
        {
            var color = image.color;
            if (duration <= 0f)
            {
                color.a = targetAlpha;
                image.color = color;
                yield break;
            }

            var startAlpha = color.a;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                image.color = color;
                yield return null;
            }

            color.a = targetAlpha;
            image.color = color;
        }

        private IEnumerator FadeCanvasGroupTo(CanvasGroup group, float targetAlpha, float duration)
        {
            if (group == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                group.alpha = targetAlpha;
                yield break;
            }

            var startAlpha = group.alpha;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            group.alpha = targetAlpha;
        }

        private void SetBlackAlpha(float alpha)
        {
            if (blackOverlayImage != null)
            {
                var color = blackOverlayImage.color;
                color.a = Mathf.Clamp01(alpha);
                blackOverlayImage.color = color;
            }
        }

        private void SetWhiteAlpha(float alpha)
        {
            if (whiteOverlayImage != null)
            {
                var color = whiteOverlayImage.color;
                color.a = Mathf.Clamp01(alpha);
                whiteOverlayImage.color = color;
            }
        }

        private void ClearIntroText()
        {
            if (introText != null)
            {
                introText.text = string.Empty;
            }

            if (introTextGroup != null)
            {
                introTextGroup.alpha = 0f;
            }
        }

        private void ClearTitle()
        {
            if (titleText != null)
            {
                titleText.text = string.Empty;
            }

            if (titleGroup != null)
            {
                titleGroup.alpha = 0f;
            }
        }
    }
}
