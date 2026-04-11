using BS.Core;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using BS.Gameplay.Story.Conditions;
using UnityEngine;

namespace BS.Gameplay.Story
{
    /// <summary>
    /// 受剧情条件控制的门交互对象。
    /// 满足条件时开门，不满足时可播放一段提示对白。
    /// </summary>
    public sealed class DoorInteractable : InteractableBase
    {
        [Header("Door Config")]
        [SerializeField] private DoorController doorController;
        [SerializeField] private FlagConditionGroup openConditions = new();
        [SerializeField] private DialogueData blockedDialogue;

        public override void Interact(PlayerInteractor interactor)
        {
            if (doorController == null)
            {
                Debug.LogWarning("DoorInteractable 缺少 DoorController。", this);
                return;
            }

            if (doorController.IsOpen)
            {
                return;
            }

            var flags = GameManager.Instance != null ? GameManager.Instance.Flags : null;
            if (flags != null && openConditions.Evaluate(flags))
            {
                doorController.Open();
                return;
            }

            if (blockedDialogue != null && GameManager.Instance != null && GameManager.Instance.Dialogue != null)
            {
                GameManager.Instance.Dialogue.StartDialogue(blockedDialogue, interactor);
            }
        }
    }
}
