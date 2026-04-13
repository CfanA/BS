using System;
using UnityEngine;

namespace BS.Gameplay.Transitions.Data
{
    public enum SceneTransitionTemplate
    {
        WhiteFlash,
        WindFadeToBlack,
        HardCutBlack,
        FullBlackHold
    }

    public enum TransitionTextRevealMode
    {
        Instant,
        Typewriter
    }

    [Serializable]
    public struct TransitionBlackTextCue
    {
        [TextArea(2, 5)]
        public string content;
        public TransitionTextRevealMode revealMode;
        public float displayDuration;
        public float postDelay;
        public bool allowSkip;
        public AudioClip syncedSfx;
    }

    [Serializable]
    public struct TransitionTitleCard
    {
        public bool enabled;
        public string title;
        public string subtitle;
        public float fadeInDuration;
        public float holdDuration;
        public float fadeOutDuration;
    }

    /// <summary>
    /// 过场模板配置。
    /// 用于跨场景复用不同的演出节奏，而不是把时序写死在脚本里。
    /// </summary>
    [CreateAssetMenu(menuName = "BS/Transitions/Scene Transition Preset", fileName = "SceneTransitionPreset_")]
    public sealed class SceneTransitionPreset : ScriptableObject
    {
        [Header("基础信息")]
        [SerializeField] private string presetId = "transition.sample";
        [SerializeField] private SceneTransitionTemplate template;
        [SerializeField] private bool lockPlayerDuringTransition = true;

        [Header("遮罩")]
        [SerializeField] private Color overlayColor = Color.black;
        [SerializeField] private float exitFadeDuration = 0.35f;
        [SerializeField] private float holdBeforeLoad = 0.3f;
        [SerializeField] private float holdAfterLoad = 0f;
        [SerializeField] private float enterFadeDuration = 0.35f;
        [SerializeField] private bool keepOverlayVisibleOnComplete;

        [Header("音频")]
        [SerializeField] private AudioClip preTransitionSfx;
        [SerializeField] private float preTransitionSfxDelay;
        [SerializeField] private AudioClip loopingAudioClip;
        [SerializeField] private bool stopLoopingAudioOnComplete = true;
        [SerializeField] [Range(0f, 1f)] private float loopingAudioVolume = 1f;

        [Header("黑场文本")]
        [SerializeField] private float blackTextStartDelay;
        [SerializeField] private float defaultTextRevealSpeed = 30f;
        [SerializeField] private TransitionBlackTextCue[] blackTextCues;

        [Header("章节标题")]
        [SerializeField] private TransitionTitleCard titleCard;

        public string PresetId => presetId;
        public SceneTransitionTemplate Template => template;
        public bool LockPlayerDuringTransition => lockPlayerDuringTransition;
        public Color OverlayColor => overlayColor;
        public float ExitFadeDuration => exitFadeDuration;
        public float HoldBeforeLoad => holdBeforeLoad;
        public float HoldAfterLoad => holdAfterLoad;
        public float EnterFadeDuration => enterFadeDuration;
        public bool KeepOverlayVisibleOnComplete => keepOverlayVisibleOnComplete;
        public AudioClip PreTransitionSfx => preTransitionSfx;
        public float PreTransitionSfxDelay => preTransitionSfxDelay;
        public AudioClip LoopingAudioClip => loopingAudioClip;
        public bool StopLoopingAudioOnComplete => stopLoopingAudioOnComplete;
        public float LoopingAudioVolume => loopingAudioVolume;
        public float BlackTextStartDelay => blackTextStartDelay;
        public float DefaultTextRevealSpeed => defaultTextRevealSpeed;
        public TransitionBlackTextCue[] BlackTextCues => blackTextCues;
        public TransitionTitleCard TitleCard => titleCard;
    }
}
