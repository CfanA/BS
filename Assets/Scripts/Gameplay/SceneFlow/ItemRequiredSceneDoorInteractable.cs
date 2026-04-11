using BS.Core;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using BS.Gameplay.Items.Data;
using UnityEngine;

namespace BS.Gameplay.SceneFlow
{
    /// <summary>
    /// 持有指定道具后才能通过的场景门。
    /// 适合“钥匙开门进入下一房间”这种叙事解谜流程。
    /// </summary>
    public sealed class ItemRequiredSceneDoorInteractable : InteractableBase
    {
        [Header("Travel")]
        [SerializeField] private SceneReference targetScene;
        [SerializeField] private string targetSpawnPointId = "entry.default";

        [Header("Item Gate")]
        [SerializeField] private ItemData requiredItem;
        [SerializeField] private bool consumeItemOnUse;

        [Header("Feedback")]
        [SerializeField] private string lockedMessageText = "The door is locked.";
        [SerializeField] private DialogueData lockedDialogue;

        public override bool CanInteract(PlayerInteractor interactor)
        {
            return base.CanInteract(interactor) && targetScene.IsValid;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null || GameManager.Instance.SceneLoader == null || GameManager.Instance.Inventory == null)
            {
                Debug.LogError("缺少场景切换或背包系统。", this);
                return;
            }

            if (!targetScene.IsValid)
            {
                Debug.LogWarning("ItemRequiredSceneDoorInteractable 未配置目标场景。", this);
                return;
            }

            if (requiredItem != null && !GameManager.Instance.Inventory.HasItem(requiredItem))
            {
                HandleLocked(interactor);
                return;
            }

            if (consumeItemOnUse && requiredItem != null)
            {
                GameManager.Instance.Inventory.RemoveItem(requiredItem, 1);
            }

            GameManager.Instance.SceneLoader.LoadScene(targetScene.SceneId, targetSpawnPointId);
        }

        private void HandleLocked(PlayerInteractor interactor)
        {
            if (lockedDialogue != null && GameManager.Instance != null && GameManager.Instance.Dialogue != null)
            {
                GameManager.Instance.Dialogue.StartDialogue(lockedDialogue, interactor);
                return;
            }

            if (!string.IsNullOrWhiteSpace(lockedMessageText))
            {
                Debug.Log(lockedMessageText, this);
            }
        }
    }
}
