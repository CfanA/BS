using BS.Core;
using BS.Gameplay.Dialogue.Data;
using UnityEngine;

namespace BS.Gameplay.Dialogue
{
    /// <summary>
    /// 监听指定对话的结束事件，并在结束后打开目标小游戏界面。
    /// </summary>
    public sealed class DialogueMiniGameLauncher : MonoBehaviour
    {
        [Header("触发条件")]
        [SerializeField] private DialogueData targetDialogue;
        [SerializeField] private string targetDialogueId;

        [Header("目标小游戏")]
        [SerializeField] private GameObject targetMiniGameRoot;

        [Header("可选：启动小游戏前隐藏这些对象")]
        [SerializeField] private GameObject[] objectsToHide;

        [Header("初始化")]
        [SerializeField] private bool hideMiniGameOnStart = true;

        private DialogueManager _dialogueManager;

        private void Start()
        {
            _dialogueManager = GameManager.Instance != null ? GameManager.Instance.Dialogue : null;
            if (_dialogueManager == null)
            {
                Debug.LogWarning("DialogueMiniGameLauncher 找不到 DialogueManager。", this);
                return;
            }

            _dialogueManager.DialogueEnded += HandleDialogueEnded;

            if (hideMiniGameOnStart && targetMiniGameRoot != null)
            {
                targetMiniGameRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_dialogueManager != null)
            {
                _dialogueManager.DialogueEnded -= HandleDialogueEnded;
            }
        }

        private void HandleDialogueEnded(DialogueData dialogueData)
        {
            if (!IsTargetDialogue(dialogueData))
            {
                return;
            }

            HideConfiguredObjects();

            if (targetMiniGameRoot != null)
            {
                targetMiniGameRoot.SetActive(true);
            }
        }

        private bool IsTargetDialogue(DialogueData dialogueData)
        {
            if (dialogueData == null)
            {
                return false;
            }

            if (targetDialogue != null)
            {
                return dialogueData == targetDialogue;
            }

            return !string.IsNullOrWhiteSpace(targetDialogueId)
                && dialogueData.DialogueId == targetDialogueId;
        }

        private void HideConfiguredObjects()
        {
            if (objectsToHide == null)
            {
                return;
            }

            foreach (var target in objectsToHide)
            {
                if (target != null)
                {
                    target.SetActive(false);
                }
            }
        }
    }
}
