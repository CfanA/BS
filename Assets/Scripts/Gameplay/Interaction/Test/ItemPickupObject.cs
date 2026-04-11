using BS.Core;
using BS.Gameplay.Items.Data;
using UnityEngine;

namespace BS.Gameplay.Interaction.Test
{
    /// <summary>
    /// 道具拾取测试对象。
    /// 交互后把 ItemData 加入 InventoryManager。
    /// </summary>
    public sealed class ItemPickupObject : InteractableBase
    {
        [Header("道具配置")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private bool hideAfterPickup = true;

        public override bool CanInteract(PlayerInteractor interactor)
        {
            if (itemData == null)
            {
                return false;
            }

            if (!base.CanInteract(interactor))
            {
                return false;
            }

            var inventory = GameManager.Instance != null ? GameManager.Instance.Inventory : null;
            if (inventory == null)
            {
                return true;
            }

            return !itemData.IsUnique || !inventory.HasItem(itemData);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (itemData == null)
            {
                Debug.LogWarning("ItemPickupObject 缺少 ItemData。", this);
                return;
            }

            if (GameManager.Instance == null || GameManager.Instance.Inventory == null)
            {
                Debug.LogError("找不到 InventoryManager，无法拾取道具。", this);
                return;
            }

            if (!GameManager.Instance.Inventory.TryAddItem(itemData))
            {
                Debug.Log($"道具未加入背包，可能是唯一道具已持有: {itemData.DisplayName}", this);
                return;
            }

            Debug.Log($"获得道具: {itemData.DisplayName}", this);

            if (hideAfterPickup)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
