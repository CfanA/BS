using BS.Core;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using UnityEngine;

namespace BS.Gameplay.Story
{
    /// <summary>
    /// 调查后设置剧情 Flag 的示例对象。
    /// 可用于查看关键物体、现场调查、剧情入口等场景。
    /// </summary>
    public sealed class InspectableFlagObject : InteractableBase
    {
        [Header("Inspect Config")]
        [SerializeField] private FlagReference discoveredFlag;
        [SerializeField] private bool triggerOnlyOnce = true;
        [SerializeField] private DialogueData inspectDialogue;

        private bool _triggeredThisSession;

        public override bool CanInteract(PlayerInteractor interactor)
        {
            if (!base.CanInteract(interactor))
            {
                return false;
            }

            if (!triggerOnlyOnce || !discoveredFlag.IsValid || GameManager.Instance == null || GameManager.Instance.Flags == null)
            {
                return true;
            }

            return !_triggeredThisSession && !GameManager.Instance.Flags.GetFlag(discoveredFlag.FlagId);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null || GameManager.Instance.Flags == null)
            {
                Debug.LogError("找不到 FlagManager，无法记录调查结果。", this);
                return;
            }

            if (discoveredFlag.IsValid)
            {
                GameManager.Instance.Flags.SetFlag(discoveredFlag.FlagId, true);
                _triggeredThisSession = true;
            }

            if (inspectDialogue != null && GameManager.Instance.Dialogue != null)
            {
                GameManager.Instance.Dialogue.StartDialogue(inspectDialogue, interactor);
            }
            else
            {
                Debug.Log($"调查对象并设置 Flag: {discoveredFlag.DisplayName}", this);
            }
        }
    }
}
