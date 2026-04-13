using System;
using System.Collections;
using BS.Core;
using BS.Foundation.Ids;
using BS.Gameplay.Interaction;
using BS.Gameplay.Player;
using BS.Gameplay.Transitions.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BS.Gameplay.Transitions
{
    /// <summary>
    /// 统一的演出型场景切换控制器。
    /// 负责播放过场遮罩、黑场文本、章节标题、音效，并与 SceneLoader 协作切场。
    /// </summary>
    public sealed class SceneTransitionController : MonoBehaviour
    {
        private sealed class RuntimeSceneTransition : ISceneTransition
        {
            private readonly SceneTransitionController _controller;
            private readonly SceneTransitionPreset _preset;

            public RuntimeSceneTransition(SceneTransitionController controller, SceneTransitionPreset preset)
            {
                _controller = controller;
                _preset = preset;
            }

            public IEnumerator PlayExit()
            {
                yield return _controller.PlayExitSequence(_preset);
            }

            public IEnumerator PlayEnter()
            {
                yield return _controller.PlayEnterSequence(_preset);
            }
        }

        [Header("生命周期")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        [Header("UI 引用")]
        [SerializeField] private GameObject root;
        [SerializeField] private Image overlayImage;
        [SerializeField] private CanvasGroup blackTextGroup;
        [SerializeField] private TMP_Text blackText;
        [SerializeField] private CanvasGroup titleGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;

        [Header("音频")]
        [SerializeField] private AudioSource oneShotAudioSource;
        [SerializeField] private AudioSource loopingAudioSource;

        [Header("玩家锁定")]
        [SerializeField] private PlayerInputReader playerInputReader;
        [SerializeField] private PlayerMotor2D playerMotor2D;
        [SerializeField] private PlayerInteractor playerInteractor;

        [Header("跳过配置")]
        [SerializeField] private KeyCode keyboardSkipKey = KeyCode.Space;
        [SerializeField] private bool allowMouseSkip = true;

        public static SceneTransitionController Instance { get; private set; }

        public bool IsPlaying { get; private set; }

        public event Action<SceneTransitionPreset, SceneTransitionRequest> TransitionStarted;
        public event Action<SceneTransitionPreset, SceneTransitionRequest> TransitionCompleted;

        private SceneTransitionPreset _activePreset;
        private SceneTransitionRequest _activeRequest;
        private bool _lockedInput;
        private bool _lockedMovement;
        private bool _lockedInteraction;
        private bool _sceneLoadCompleted;
        private bool _transitionCompleted;

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
                DontDestroyOnLoad(gameObject);
            }

            ResetVisualState();
            SetRootVisible(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool Play(SceneTransitionPreset preset)
        {
            return Play(preset, new SceneTransitionRequest());
        }

        public bool Play(SceneTransitionPreset preset, SceneTransitionRequest request)
        {
            if (preset == null || IsPlaying)
            {
                return false;
            }

            StartCoroutine(PlayRoutine(preset, request ?? new SceneTransitionRequest()));
            return true;
        }

        public bool Play(SceneTransitionPreset preset, string sceneName, string spawnPointId = null)
        {
            return Play(preset, new SceneTransitionRequest(sceneName, spawnPointId));
        }

        private IEnumerator PlayRoutine(SceneTransitionPreset preset, SceneTransitionRequest request)
        {
            IsPlaying = true;
            _activePreset = preset;
            _activeRequest = request;
            _sceneLoadCompleted = false;
            _transitionCompleted = false;

            ResetVisualState();
            SetRootVisible(true);
            StartLoopingAudioIfNeeded(preset);
            ApplyPlayerLock(preset, false);
            TransitionStarted?.Invoke(preset, request);

            if (request.BeforeSceneLoad != null)
            {
                request.BeforeSceneLoad.Invoke();
            }

            if (request.HasTargetScene && GameManager.Instance != null && GameManager.Instance.SceneLoader != null)
            {
                yield return PlayWithSceneLoadRoutine(preset, request);
            }
            else
            {
                var runtimeTransition = new RuntimeSceneTransition(this, preset);
                yield return runtimeTransition.PlayExit();
                yield return runtimeTransition.PlayEnter();
            }

            CompleteTransition();
        }

        private IEnumerator PlayWithSceneLoadRoutine(SceneTransitionPreset preset, SceneTransitionRequest request)
        {
            var sceneLoader = GameManager.Instance.SceneLoader;
            sceneLoader.SceneLoaded += HandleSceneLoaded;
            sceneLoader.AfterSceneLoaded += HandleAfterSceneLoaded;

            sceneLoader.LoadScene(
                request.TargetSceneId,
                request.TargetSpawnPointId,
                new RuntimeSceneTransition(this, preset));

            while (!_transitionCompleted)
            {
                yield return null;
            }

            sceneLoader.SceneLoaded -= HandleSceneLoaded;
            sceneLoader.AfterSceneLoaded -= HandleAfterSceneLoaded;
        }

        private void HandleSceneLoaded(SceneId sceneId)
        {
            _sceneLoadCompleted = true;
            ApplyPlayerLock(_activePreset, true);
            _activeRequest?.AfterSceneLoaded?.Invoke();
        }

        private void HandleAfterSceneLoaded(SceneId sceneId)
        {
            _transitionCompleted = true;
        }

        private IEnumerator PlayExitSequence(SceneTransitionPreset preset)
        {
            if (preset == null)
            {
                yield break;
            }

            PrepareOverlay(preset.OverlayColor);
            ClearBlackText();
            ClearTitle();

            if (preset.PreTransitionSfx != null && oneShotAudioSource != null)
            {
                oneShotAudioSource.PlayOneShot(preset.PreTransitionSfx);
            }

            if (preset.PreTransitionSfxDelay > 0f)
            {
                yield return new WaitForSeconds(preset.PreTransitionSfxDelay);
            }

            switch (preset.Template)
            {
                case SceneTransitionTemplate.HardCutBlack:
                    SetOverlayAlpha(1f);
                    break;

                default:
                    yield return FadeOverlayTo(1f, preset.ExitFadeDuration);
                    break;
            }

            if (preset.HoldBeforeLoad > 0f)
            {
                yield return new WaitForSeconds(preset.HoldBeforeLoad);
            }
        }

        private IEnumerator PlayEnterSequence(SceneTransitionPreset preset)
        {
            if (preset == null)
            {
                yield break;
            }

            ApplyPlayerLock(preset, true);

            if (preset.HoldAfterLoad > 0f)
            {
                yield return new WaitForSeconds(preset.HoldAfterLoad);
            }

            yield return PlayBlackTextSequence(preset);
            yield return PlayTitleCard(preset);

            if (!preset.KeepOverlayVisibleOnComplete)
            {
                yield return FadeOverlayTo(0f, preset.EnterFadeDuration);
            }
        }

        private IEnumerator PlayBlackTextSequence(SceneTransitionPreset preset)
        {
            var cues = preset.BlackTextCues;
            if (cues == null || cues.Length == 0 || blackText == null || blackTextGroup == null)
            {
                yield break;
            }

            if (preset.BlackTextStartDelay > 0f)
            {
                yield return new WaitForSeconds(preset.BlackTextStartDelay);
            }

            for (var i = 0; i < cues.Length; i++)
            {
                var cue = cues[i];
                if (string.IsNullOrWhiteSpace(cue.content))
                {
                    continue;
                }

                if (cue.syncedSfx != null && oneShotAudioSource != null)
                {
                    oneShotAudioSource.PlayOneShot(cue.syncedSfx);
                }

                blackTextGroup.alpha = 1f;
                yield return RevealText(cue.content, cue.revealMode, preset.DefaultTextRevealSpeed, cue.allowSkip);
                yield return WaitWithOptionalSkip(cue.displayDuration, cue.allowSkip);
                blackText.text = string.Empty;

                if (cue.postDelay > 0f)
                {
                    yield return WaitWithOptionalSkip(cue.postDelay, cue.allowSkip);
                }
            }

            blackTextGroup.alpha = 0f;
            blackText.text = string.Empty;
        }

        private IEnumerator PlayTitleCard(SceneTransitionPreset preset)
        {
            var titleCard = preset.TitleCard;
            if (!titleCard.enabled || titleGroup == null)
            {
                yield break;
            }

            if (titleText != null)
            {
                titleText.text = titleCard.title ?? string.Empty;
            }

            if (subtitleText != null)
            {
                subtitleText.text = titleCard.subtitle ?? string.Empty;
            }

            titleGroup.alpha = 0f;
            yield return FadeCanvasGroup(titleGroup, 1f, titleCard.fadeInDuration);
            yield return new WaitForSeconds(Mathf.Max(0f, titleCard.holdDuration));
            yield return FadeCanvasGroup(titleGroup, 0f, titleCard.fadeOutDuration);
        }

        private IEnumerator RevealText(string content, TransitionTextRevealMode revealMode, float revealSpeed, bool allowSkip)
        {
            if (blackText == null)
            {
                yield break;
            }

            if (revealMode == TransitionTextRevealMode.Instant || revealSpeed <= 0f)
            {
                blackText.text = content;
                yield break;
            }

            blackText.text = string.Empty;
            var interval = 1f / revealSpeed;

            for (var i = 1; i <= content.Length; i++)
            {
                blackText.text = content.Substring(0, i);
                if (allowSkip && WasSkipPressed())
                {
                    blackText.text = content;
                    yield break;
                }

                if (i < content.Length)
                {
                    yield return new WaitForSeconds(interval);
                }
            }
        }

        private IEnumerator WaitWithOptionalSkip(float duration, bool allowSkip)
        {
            if (duration <= 0f)
            {
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (allowSkip && WasSkipPressed())
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator FadeOverlayTo(float targetAlpha, float duration)
        {
            if (overlayImage == null)
            {
                yield break;
            }

            var color = overlayImage.color;
            if (duration <= 0f)
            {
                color.a = targetAlpha;
                overlayImage.color = color;
                yield break;
            }

            var startAlpha = color.a;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                overlayImage.color = color;
                yield return null;
            }

            color.a = targetAlpha;
            overlayImage.color = color;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, float duration)
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

        private void PrepareOverlay(Color overlayColor)
        {
            if (overlayImage == null)
            {
                return;
            }

            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (overlayImage == null)
            {
                return;
            }

            var color = overlayImage.color;
            color.a = Mathf.Clamp01(alpha);
            overlayImage.color = color;
        }

        private void ResetVisualState()
        {
            if (overlayImage != null)
            {
                var color = overlayImage.color;
                color.a = 0f;
                overlayImage.color = color;
            }

            ClearBlackText();
            ClearTitle();
        }

        private void ClearBlackText()
        {
            if (blackText != null)
            {
                blackText.text = string.Empty;
            }

            if (blackTextGroup != null)
            {
                blackTextGroup.alpha = 0f;
            }
        }

        private void ClearTitle()
        {
            if (titleText != null)
            {
                titleText.text = string.Empty;
            }

            if (subtitleText != null)
            {
                subtitleText.text = string.Empty;
            }

            if (titleGroup != null)
            {
                titleGroup.alpha = 0f;
            }
        }

        private void SetRootVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }

        private void StartLoopingAudioIfNeeded(SceneTransitionPreset preset)
        {
            if (preset == null || preset.LoopingAudioClip == null || loopingAudioSource == null)
            {
                return;
            }

            loopingAudioSource.clip = preset.LoopingAudioClip;
            loopingAudioSource.volume = preset.LoopingAudioVolume;
            loopingAudioSource.loop = true;
            loopingAudioSource.Play();
        }

        private void StopLoopingAudioIfNeeded(SceneTransitionPreset preset)
        {
            if (preset == null || !preset.StopLoopingAudioOnComplete || loopingAudioSource == null)
            {
                return;
            }

            loopingAudioSource.Stop();
            loopingAudioSource.clip = null;
        }

        private void ApplyPlayerLock(SceneTransitionPreset preset, bool rebind)
        {
            if (preset == null || !preset.LockPlayerDuringTransition)
            {
                return;
            }

            if (rebind)
            {
                ReleasePlayerLock();
            }

            TryBindPlayerReferences();

            if (playerInputReader != null && !_lockedInput)
            {
                playerInputReader.PushInputLock();
                _lockedInput = true;
            }

            if (playerMotor2D != null && !_lockedMovement)
            {
                playerMotor2D.PushMovementLock();
                _lockedMovement = true;
            }

            if (playerInteractor != null && !_lockedInteraction)
            {
                playerInteractor.PushInteractionLock();
                _lockedInteraction = true;
            }
        }

        private void ReleasePlayerLock()
        {
            if (playerInteractor != null && _lockedInteraction)
            {
                playerInteractor.PopInteractionLock();
                _lockedInteraction = false;
            }

            if (playerMotor2D != null && _lockedMovement)
            {
                playerMotor2D.PopMovementLock();
                _lockedMovement = false;
            }

            if (playerInputReader != null && _lockedInput)
            {
                playerInputReader.PopInputLock();
                _lockedInput = false;
            }
        }

        private void TryBindPlayerReferences()
        {
            if (playerInputReader == null)
            {
                playerInputReader = FindFirstObjectByType<PlayerInputReader>(FindObjectsInactive.Exclude);
            }

            if (playerMotor2D == null)
            {
                playerMotor2D = FindFirstObjectByType<PlayerMotor2D>(FindObjectsInactive.Exclude);
            }

            if (playerInteractor == null)
            {
                playerInteractor = FindFirstObjectByType<PlayerInteractor>(FindObjectsInactive.Exclude);
            }
        }

        private bool WasSkipPressed()
        {
            return Input.GetKeyDown(keyboardSkipKey) || allowMouseSkip && Input.GetMouseButtonDown(0);
        }

        private void CompleteTransition()
        {
            StopLoopingAudioIfNeeded(_activePreset);

            if (_activePreset == null || !_activePreset.KeepOverlayVisibleOnComplete)
            {
                SetRootVisible(false);
                ResetVisualState();
            }

            if (_activeRequest == null || _activeRequest.UnlockPlayerOnComplete)
            {
                ReleasePlayerLock();
            }

            TransitionCompleted?.Invoke(_activePreset, _activeRequest);
            _activeRequest?.Completed?.Invoke();

            _activePreset = null;
            _activeRequest = null;
            _sceneLoadCompleted = false;
            _transitionCompleted = false;
            IsPlaying = false;
        }
    }
}
