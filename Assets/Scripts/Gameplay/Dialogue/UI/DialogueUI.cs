using System.Collections;
using BS.Core;
using BS.Gameplay.Dialogue.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BS.Gameplay.Dialogue.UI
{
    /// <summary>
    /// 对话 UI 控制器。
    /// 只负责监听 DialogueManager 事件并刷新界面，不参与对话流程控制。
    /// </summary>
    public sealed class DialogueUI : MonoBehaviour
    {
        [Header("依赖引用")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text pageIndicatorText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private GameObject portraitRoot;

        [Header("Typewriter")]
        [SerializeField] private bool enableTypewriter = true;
        [SerializeField] private float charactersPerSecond = 24f;

        private bool _isSubscribed;
        private Coroutine _typewriterRoutine;

        private void Awake()
        {
            TryBindDialogueManager();
        }

        private void OnEnable()
        {
            TryBindDialogueManager();

            if (dialogueManager == null || _isSubscribed)
            {
                return;
            }

            dialogueManager.DialogueStarted += HandleDialogueStarted;
            dialogueManager.LineChanged += HandleLineChanged;
            dialogueManager.DialogueEnded += HandleDialogueEnded;
            _isSubscribed = true;
        }

        private void OnDisable()
        {
            StopTypewriter();

            if (dialogueManager == null || !_isSubscribed)
            {
                return;
            }

            dialogueManager.DialogueStarted -= HandleDialogueStarted;
            dialogueManager.LineChanged -= HandleLineChanged;
            dialogueManager.DialogueEnded -= HandleDialogueEnded;
            _isSubscribed = false;
        }

        private void Start()
        {
            RefreshPortrait(null);
            SetRootVisible(false);
        }

        private void HandleDialogueStarted(DialogueData dialogueData)
        {
            SetRootVisible(true);
        }

        private void HandleLineChanged(DialogueLine line, int lineIndex, int totalLineCount)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = line != null ? line.SpeakerName : string.Empty;
            }

            RefreshContent(line);

            if (pageIndicatorText != null)
            {
                pageIndicatorText.text = $"{lineIndex + 1}/{totalLineCount}";
            }

            RefreshPortrait(line);
        }

        private void HandleDialogueEnded(DialogueData dialogueData)
        {
            StopTypewriter();

            if (speakerNameText != null)
            {
                speakerNameText.text = string.Empty;
            }

            if (contentText != null)
            {
                contentText.text = string.Empty;
            }

            if (pageIndicatorText != null)
            {
                pageIndicatorText.text = string.Empty;
            }

            RefreshPortrait(null);
            SetRootVisible(false);
        }

        private void SetRootVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }

        private void RefreshPortrait(DialogueLine line)
        {
            if (portraitImage == null)
            {
                if (portraitRoot != null)
                {
                    portraitRoot.SetActive(false);
                }

                return;
            }

            var portrait = line != null ? line.Portrait : null;
            var hasPortrait = portrait != null;

            portraitImage.sprite = portrait;

            if (portraitRoot != null)
            {
                portraitRoot.SetActive(hasPortrait);
                return;
            }

            portraitImage.gameObject.SetActive(hasPortrait);
        }

        private void RefreshContent(DialogueLine line)
        {
            if (contentText == null)
            {
                return;
            }

            StopTypewriter();

            var content = line != null ? line.Content : string.Empty;
            if (!enableTypewriter || string.IsNullOrEmpty(content) || charactersPerSecond <= 0f)
            {
                contentText.text = content;
                return;
            }

            _typewriterRoutine = StartCoroutine(PlayTypewriterRoutine(content));
        }

        private IEnumerator PlayTypewriterRoutine(string content)
        {
            contentText.text = string.Empty;

            var interval = 1f / charactersPerSecond;
            for (var i = 1; i <= content.Length; i++)
            {
                contentText.text = content.Substring(0, i);
                if (i < content.Length)
                {
                    yield return new WaitForSeconds(interval);
                }
            }

            _typewriterRoutine = null;
        }

        private void StopTypewriter()
        {
            if (_typewriterRoutine == null)
            {
                return;
            }

            StopCoroutine(_typewriterRoutine);
            _typewriterRoutine = null;
        }

        private void TryBindDialogueManager()
        {
            if (dialogueManager == null && GameManager.Instance != null)
            {
                dialogueManager = GameManager.Instance.Dialogue;
            }
        }
    }
}
