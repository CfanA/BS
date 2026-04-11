using BS.Core;
using BS.Gameplay.Clues.Data;
using UnityEngine;

namespace BS.Gameplay.Interaction.Test
{
    /// <summary>
    /// 线索记录测试对象。
    /// 交互后把 ClueData 记录到 ClueNotebookManager。
    /// </summary>
    public sealed class CluePickupObject : InteractableBase
    {
        [Header("线索配置")]
        [SerializeField] private ClueData clueData;
        [SerializeField] private bool hideAfterPickup = true;

        public override bool CanInteract(PlayerInteractor interactor)
        {
            if (clueData == null)
            {
                return false;
            }

            if (!base.CanInteract(interactor))
            {
                return false;
            }

            var notebook = GameManager.Instance != null ? GameManager.Instance.ClueNotebook : null;
            if (notebook == null)
            {
                return true;
            }

            return !notebook.HasClue(clueData);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (clueData == null)
            {
                Debug.LogWarning("CluePickupObject 缺少 ClueData。", this);
                return;
            }

            if (GameManager.Instance == null || GameManager.Instance.ClueNotebook == null)
            {
                Debug.LogError("找不到 ClueNotebookManager，无法记录线索。", this);
                return;
            }

            if (!GameManager.Instance.ClueNotebook.TryAddClue(clueData))
            {
                Debug.Log($"线索未记录，可能已存在: {clueData.DisplayName}", this);
                return;
            }

            Debug.Log($"获得线索: {clueData.DisplayName}", this);

            if (hideAfterPickup)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
