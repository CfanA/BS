using BS.Gameplay.Dialogue.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BS.Core;

namespace BS.Gameplay.Dialogue.UI
{
    /// <summary>
    /// 对话 UI 控制器。
    /// 只负责订阅 DialogueManager 事件并刷新界面，不参与对话流程控制。
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

        private bool _isSubscribed;

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

            if (contentText != null)
            {
                contentText.text = line != null ? line.Content : string.Empty;
            }

            if (pageIndicatorText != null)
            {
                pageIndicatorText.text = $"{lineIndex + 1}/{totalLineCount}";
            }

            RefreshPortrait(line);
        }

        private void HandleDialogueEnded(DialogueData dialogueData)
        {
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

        private void TryBindDialogueManager()
        {
            if (dialogueManager == null && GameManager.Instance != null)
            {
                dialogueManager = GameManager.Instance.Dialogue;
            }
        }
    }
}
