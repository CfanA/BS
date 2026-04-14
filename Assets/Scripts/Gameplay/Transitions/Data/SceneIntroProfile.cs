using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BS.Gameplay.Transitions.Data
{
    public enum SceneIntroTextDisplayMode
    {
        None,
        Instant,
        Typewriter
    }

    [Serializable]
    public sealed class SceneIntroProfile
    {
        [SerializeField, FormerlySerializedAs("sceneName")] private string 场景名;
        [SerializeField, FormerlySerializedAs("initialBlackHold")] private float 初始黑场停留 = 0.2f;
        [SerializeField, FormerlySerializedAs("enableWhiteFlash")] private bool 启用闪白;
        [SerializeField, FormerlySerializedAs("whiteFlashDuration")] private float 闪白持续时间 = 0.2f;

        [SerializeField, FormerlySerializedAs("introText")] [TextArea(2, 4)] private string 介绍文字;
        [SerializeField, FormerlySerializedAs("textDisplayMode")] private SceneIntroTextDisplayMode 文字显示模式 = SceneIntroTextDisplayMode.None;
        [SerializeField, FormerlySerializedAs("typewriterCharactersPerSecond")] private float 打字机每秒字符数 = 28f;
        [SerializeField, FormerlySerializedAs("enableTextFadeIn")] private bool 启用文字淡入 = true;
        [SerializeField, FormerlySerializedAs("textFadeInDuration")] private float 文字淡入时长 = 0.2f;
        [SerializeField, FormerlySerializedAs("textHoldDuration")] private float 文字停留时长 = 1.5f;
        [SerializeField, FormerlySerializedAs("enableTextFadeOut")] private bool 启用文字淡出 = true;
        [SerializeField, FormerlySerializedAs("textFadeOutDuration")] private float 文字淡出时长 = 0.2f;

        [SerializeField, FormerlySerializedAs("revealDuration")] private float 场景揭示时长 = 0.35f;

        [SerializeField, FormerlySerializedAs("titleText")] private string 标题文字;
        [SerializeField, FormerlySerializedAs("enableTitleFadeIn")] private bool 启用标题淡入 = true;
        [SerializeField, FormerlySerializedAs("titleFadeInDuration")] private float 标题淡入时长 = 0.3f;
        [SerializeField, FormerlySerializedAs("titleHoldDuration")] private float 标题停留时长 = 1.5f;
        [SerializeField, FormerlySerializedAs("enableTitleFadeOut")] private bool 启用标题淡出 = true;
        [SerializeField, FormerlySerializedAs("titleFadeOutDuration")] private float 标题淡出时长 = 0.3f;

        public string SceneName
        {
            get => 场景名;
            set => 场景名 = value;
        }

        public float InitialBlackHold
        {
            get => 初始黑场停留;
            set => 初始黑场停留 = value;
        }

        public bool EnableWhiteFlash
        {
            get => 启用闪白;
            set => 启用闪白 = value;
        }

        public float WhiteFlashDuration
        {
            get => 闪白持续时间;
            set => 闪白持续时间 = value;
        }

        public string IntroText
        {
            get => 介绍文字;
            set => 介绍文字 = value;
        }

        public SceneIntroTextDisplayMode TextDisplayMode
        {
            get => 文字显示模式;
            set => 文字显示模式 = value;
        }

        public float TypewriterCharactersPerSecond
        {
            get => 打字机每秒字符数;
            set => 打字机每秒字符数 = value;
        }

        public bool EnableTextFadeIn
        {
            get => 启用文字淡入;
            set => 启用文字淡入 = value;
        }

        public float TextFadeInDuration
        {
            get => 文字淡入时长;
            set => 文字淡入时长 = value;
        }

        public float TextHoldDuration
        {
            get => 文字停留时长;
            set => 文字停留时长 = value;
        }

        public bool EnableTextFadeOut
        {
            get => 启用文字淡出;
            set => 启用文字淡出 = value;
        }

        public float TextFadeOutDuration
        {
            get => 文字淡出时长;
            set => 文字淡出时长 = value;
        }

        public float RevealDuration
        {
            get => 场景揭示时长;
            set => 场景揭示时长 = value;
        }

        public string TitleText
        {
            get => 标题文字;
            set => 标题文字 = value;
        }

        public bool EnableTitleFadeIn
        {
            get => 启用标题淡入;
            set => 启用标题淡入 = value;
        }

        public float TitleFadeInDuration
        {
            get => 标题淡入时长;
            set => 标题淡入时长 = value;
        }

        public float TitleHoldDuration
        {
            get => 标题停留时长;
            set => 标题停留时长 = value;
        }

        public bool EnableTitleFadeOut
        {
            get => 启用标题淡出;
            set => 启用标题淡出 = value;
        }

        public float TitleFadeOutDuration
        {
            get => 标题淡出时长;
            set => 标题淡出时长 = value;
        }
    }
}
