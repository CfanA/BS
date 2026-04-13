using System.Collections;
using System.Collections.Generic;
using BS.Core;
using BS.Gameplay.Items;
using BS.Gameplay.Items.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BS.Gameplay.UI
{
    /// <summary>
    /// 监听背包新增事件，顺序播放拾取提示。
    /// </summary>
    public sealed class ItemPickupToastUI : MonoBehaviour
    {
        private enum HideMode
        {
            ByDuration,
            OnDialogueEnded
        }

        [System.Serializable]
        private struct ItemToastDisplayOverride
        {
            public ItemData itemData;
            public HideMode hideMode;
            public float displayDuration;
        }

        private readonly Queue<ItemToastRequest> _pendingRequests = new();

        [Header("依赖引用")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private Dialogue.DialogueManager dialogueManager;
        [SerializeField] private GameObject root;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text amountText;

        [Header("显示配置")]
        [SerializeField] private HideMode hideMode = HideMode.ByDuration;
        [SerializeField] private float displayDuration = 1.5f;
        [SerializeField] private ItemToastDisplayOverride[] itemDisplayOverrides;

        private Coroutine _playRoutine;
        private bool _isSubscribed;

        private void Awake()
        {
            TryBindInventoryManager();
            TryBindDialogueManager();
        }

        private void OnEnable()
        {
            TryBindInventoryManager();
            TryBindDialogueManager();

            if (inventoryManager == null || _isSubscribed)
            {
                return;
            }

            inventoryManager.ItemAdded += HandleItemAdded;
            _isSubscribed = true;
        }

        private void OnDisable()
        {
            if (inventoryManager != null && _isSubscribed)
            {
                inventoryManager.ItemAdded -= HandleItemAdded;
                _isSubscribed = false;
            }

            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }

            _pendingRequests.Clear();
            SetRootVisible(false);
        }

        private void Start()
        {
            SetRootVisible(false);
            RefreshView(null, 0);
        }

        private void HandleItemAdded(ItemData itemData, int amount)
        {
            if (itemData == null || amount <= 0)
            {
                return;
            }

            _pendingRequests.Enqueue(new ItemToastRequest(itemData, amount));

            if (_playRoutine == null)
            {
                _playRoutine = StartCoroutine(PlayQueueRoutine());
            }
        }

        private IEnumerator PlayQueueRoutine()
        {
            while (_pendingRequests.Count > 0)
            {
                var request = _pendingRequests.Dequeue();
                RefreshView(request.ItemData, request.Amount);
                SetRootVisible(true);
                yield return WaitForHideCondition(request.ItemData);

                SetRootVisible(false);
            }

            RefreshView(null, 0);
            _playRoutine = null;
        }

        private void RefreshView(ItemData itemData, int amount)
        {
            if (itemNameText != null)
            {
                itemNameText.text = itemData != null ? itemData.DisplayName : string.Empty;
            }

            if (amountText != null)
            {
                amountText.text = itemData != null ? $"获得 x{amount}" : string.Empty;
            }

            RefreshIcon(itemData);
        }

        private void RefreshIcon(ItemData itemData)
        {
            if (iconImage == null)
            {
                return;
            }

            var icon = itemData != null ? itemData.Icon : null;
            var hasIcon = icon != null;

            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(hasIcon);
        }

        private void SetRootVisible(bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }

        private void TryBindInventoryManager()
        {
            if (inventoryManager == null && GameManager.Instance != null)
            {
                inventoryManager = GameManager.Instance.Inventory;
            }
        }

        private void TryBindDialogueManager()
        {
            if (dialogueManager == null && GameManager.Instance != null)
            {
                dialogueManager = GameManager.Instance.Dialogue;
            }
        }

        private IEnumerator WaitForHideCondition(ItemData itemData)
        {
            var resolvedHideMode = hideMode;
            var resolvedDuration = displayDuration;
            ResolveDisplaySettings(itemData, ref resolvedHideMode, ref resolvedDuration);

            switch (resolvedHideMode)
            {
                case HideMode.OnDialogueEnded:
                    if (dialogueManager != null && dialogueManager.IsPlaying)
                    {
                        yield return new WaitUntil(() => dialogueManager == null || !dialogueManager.IsPlaying);
                        yield break;
                    }
                    break;
            }

            var duration = Mathf.Max(0.1f, resolvedDuration);
            yield return new WaitForSeconds(duration);
        }

        private void ResolveDisplaySettings(ItemData itemData, ref HideMode resolvedHideMode, ref float resolvedDuration)
        {
            if (itemData == null || itemDisplayOverrides == null || itemDisplayOverrides.Length == 0)
            {
                return;
            }

            for (var i = 0; i < itemDisplayOverrides.Length; i++)
            {
                var itemOverride = itemDisplayOverrides[i];
                if (itemOverride.itemData != itemData)
                {
                    continue;
                }

                resolvedHideMode = itemOverride.hideMode;
                resolvedDuration = itemOverride.displayDuration;
                return;
            }
        }

        private readonly struct ItemToastRequest
        {
            public ItemToastRequest(ItemData itemData, int amount)
            {
                ItemData = itemData;
                Amount = amount;
            }

            public ItemData ItemData { get; }
            public int Amount { get; }
        }
    }
}
