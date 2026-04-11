using System;
using BS.Core;
using BS.Foundation.Ids;
using UnityEngine;

namespace BS.Gameplay.Dialogue.Test
{
    /// <summary>
    /// 对话行事件示例响应器。
    /// 当某句对白配置了事件键时，可在这里映射为剧情 Flag 或其他逻辑。
    /// </summary>
    public sealed class DialogueFlagEventResponder : MonoBehaviour
    {
        [Serializable]
        private sealed class EventFlagBinding
        {
            public string eventKey;
            public string flagId;
            public bool flagValue = true;
        }

        [Header("依赖引用")]
        [SerializeField] private DialogueManager dialogueManager;

        [Header("事件映射")]
        [SerializeField] private EventFlagBinding[] bindings;

        private void Awake()
        {
            if (dialogueManager == null && GameManager.Instance != null)
            {
                dialogueManager = GameManager.Instance.Dialogue;
            }
        }

        private void OnEnable()
        {
            if (dialogueManager == null)
            {
                return;
            }

            dialogueManager.LineEventTriggered += HandleLineEventTriggered;
        }

        private void OnDisable()
        {
            if (dialogueManager == null)
            {
                return;
            }

            dialogueManager.LineEventTriggered -= HandleLineEventTriggered;
        }

        private void HandleLineEventTriggered(string eventKey, Data.DialogueLine line, Data.DialogueData dialogueData)
        {
            if (bindings == null || bindings.Length == 0 || GameManager.Instance == null || GameManager.Instance.Flags == null)
            {
                return;
            }

            for (var i = 0; i < bindings.Length; i++)
            {
                var binding = bindings[i];
                if (binding == null || !string.Equals(binding.eventKey, eventKey, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(binding.flagId))
                {
                    GameManager.Instance.Flags.SetFlag(new FlagId(binding.flagId), binding.flagValue);
                    Debug.Log($"对话事件触发剧情 Flag: {binding.flagId} = {binding.flagValue}", this);
                }
            }
        }
    }
}
