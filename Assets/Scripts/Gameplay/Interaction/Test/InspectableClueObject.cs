using BS.Core;
using BS.Gameplay.Clues.Data;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using UnityEngine;

namespace BS.Gameplay.Interaction.Test
{
    /// <summary>
    /// 调查后记录线索的对象。
    /// 适合旧照片、笔记、墙上涂鸦这类“查看即获得线索”的场景物。
    /// </summary>
    public sealed class InspectableClueObject : InteractableBase
    {
        [Header("线索配置")]
        [SerializeField] private ClueData clueData;
        [SerializeField] private bool triggerOnlyOnce = true;
        [SerializeField] private DialogueData inspectDialogue;

        private bool _triggeredThisSession;

        public override bool CanInteract(PlayerInteractor interactor)
        {
            if (!base.CanInteract(interactor))
            {
                return false;
            }

            if (!triggerOnlyOnce || clueData == null || GameManager.Instance == null || GameManager.Instance.ClueNotebook == null)
            {
                return true;
            }

            return !_triggeredThisSession && !GameManager.Instance.ClueNotebook.HasClue(clueData);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (clueData == null)
            {
                Debug.LogWarning("InspectableClueObject 缺少 ClueData。", this);
                return;
            }

            if (GameManager.Instance == null || GameManager.Instance.ClueNotebook == null)
            {
                Debug.LogError("找不到 ClueNotebookManager，无法记录线索。", this);
                return;
            }

            GameManager.Instance.ClueNotebook.TryAddClue(clueData);
            _triggeredThisSession = true;

            if (inspectDialogue != null && GameManager.Instance.Dialogue != null)
            {
                GameManager.Instance.Dialogue.StartDialogue(inspectDialogue, interactor);
            }
            else
            {
                Debug.Log($"调查并记录线索: {clueData.DisplayName}", this);
            }
        }
    }
}
