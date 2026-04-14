using BS.Core;
using BS.Gameplay.Items.Data;
using UnityEngine;

namespace BS.Gameplay.Items
{
    /// <summary>
    /// 根据玩家是否已拥有某个道具，控制场景物体显示隐藏。
    /// 适合“对白直接给道具后，场景中的对应物件消失”的需求。
    /// </summary>
    public sealed class SceneItemOwnedVisibility : MonoBehaviour
    {
        [Header("道具配置")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int requiredAmount = 1;

        [Header("显示配置")]
        [SerializeField] private GameObject targetObject;
        [SerializeField] private bool hideWhenOwned = true;
        [SerializeField] private bool refreshOnStart = true;

        [Header("依赖引用")]
        [SerializeField] private InventoryManager inventoryManager;

        private bool _isSubscribed;

        private void Reset()
        {
            if (targetObject == null)
            {
                targetObject = gameObject;
            }
        }

        private void Awake()
        {
            TryBindInventoryManager();
        }

        private void OnEnable()
        {
            TryBindInventoryManager();

            if (inventoryManager != null && !_isSubscribed)
            {
                inventoryManager.InventoryChanged += RefreshVisibility;
                _isSubscribed = true;
            }
        }

        private void Start()
        {
            if (refreshOnStart)
            {
                RefreshVisibility();
            }
        }

        private void OnDisable()
        {
            if (inventoryManager != null && _isSubscribed)
            {
                inventoryManager.InventoryChanged -= RefreshVisibility;
                _isSubscribed = false;
            }
        }

        [ContextMenu("Refresh Visibility")]
        public void RefreshVisibility()
        {
            if (targetObject == null || itemData == null)
            {
                return;
            }

            TryBindInventoryManager();
            if (inventoryManager == null)
            {
                return;
            }

            var isOwned = inventoryManager.HasItem(itemData, Mathf.Max(1, requiredAmount));
            var shouldShow = hideWhenOwned ? !isOwned : isOwned;
            targetObject.SetActive(shouldShow);
        }

        private void TryBindInventoryManager()
        {
            if (inventoryManager == null && GameManager.Instance != null)
            {
                inventoryManager = GameManager.Instance.Inventory;
            }
        }
    }
}
