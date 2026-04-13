using BS.Core;
using BS.Foundation.Ids;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using UnityEngine;

namespace BS.Gameplay.Dialogue.Test
{
    /// <summary>
    /// 玩家进入触发区后自动播放一段对白。
    /// 适用于一次性剧情触发，不参与复杂交互逻辑。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class DialogueTriggerZone : MonoBehaviour
    {
        [Header("对白配置")]
        [SerializeField] private DialogueData dialogueData;

        [Header("触发配置")]
        [SerializeField] private bool triggerOnce = true;

        [Header("触发后 Flag")]
        [SerializeField] private string triggeredFlagId;
        [SerializeField] private bool triggeredFlagValue = true;

        private bool _hasTriggered;

        private void Reset()
        {
            var triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggerOnce && _hasTriggered)
            {
                return;
            }

            if (dialogueData == null)
            {
                Debug.LogWarning("DialogueTriggerZone 缺少 DialogueData。", this);
                return;
            }

            var interactor = other.GetComponentInParent<PlayerInteractor>();
            if (interactor == null)
            {
                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Dialogue == null)
            {
                Debug.LogError("找不到 DialogueManager，无法触发对白。", this);
                return;
            }

            if (!gameManager.Dialogue.StartDialogue(dialogueData, interactor))
            {
                return;
            }

            _hasTriggered = true;
            TrySetTriggeredFlag(gameManager);
        }

        private void TrySetTriggeredFlag(GameManager gameManager)
        {
            if (gameManager == null || gameManager.Flags == null || string.IsNullOrWhiteSpace(triggeredFlagId))
            {
                return;
            }

            gameManager.Flags.SetFlag(new FlagId(triggeredFlagId), triggeredFlagValue);
        }
    }
}
