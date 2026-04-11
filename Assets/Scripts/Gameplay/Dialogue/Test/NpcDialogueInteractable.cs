using BS.Core;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using UnityEngine;

namespace BS.Gameplay.Dialogue.Test
{
    /// <summary>
    /// NPC 对话示例。
    /// 通过统一交互系统启动一段线性对白。
    /// </summary>
    public sealed class NpcDialogueInteractable : InteractableBase
    {
        [Header("对话配置")]
        [SerializeField] private DialogueData dialogueData;

        public override bool CanInteract(PlayerInteractor interactor)
        {
            if (!base.CanInteract(interactor))
            {
                return false;
            }

            var dialogueManager = GameManager.Instance != null ? GameManager.Instance.Dialogue : null;
            return dialogueData != null && (dialogueManager == null || !dialogueManager.IsPlaying);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (dialogueData == null)
            {
                Debug.LogWarning("NpcDialogueInteractable 缺少 DialogueData。", this);
                return;
            }

            if (GameManager.Instance == null || GameManager.Instance.Dialogue == null)
            {
                Debug.LogError("找不到 DialogueManager，无法开始对白。", this);
                return;
            }

            GameManager.Instance.Dialogue.StartDialogue(dialogueData, interactor);
        }
    }
}
