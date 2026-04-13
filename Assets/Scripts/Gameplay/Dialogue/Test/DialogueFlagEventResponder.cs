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
            public bool applyOnDialogueEnded;
        }

        [Header("依赖引用")]
        [SerializeField] private DialogueManager dialogueManager;

        [Header("事件映射")]
        [SerializeField] private EventFlagBinding[] bindings;

        private EventFlagBinding _pendingBinding;

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
            dialogueManager.DialogueEnded += HandleDialogueEnded;
        }

        private void OnDisable()
        {
            if (dialogueManager == null)
            {
                return;
            }

            dialogueManager.LineEventTriggered -= HandleLineEventTriggered;
            dialogueManager.DialogueEnded -= HandleDialogueEnded;
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
                    if (binding.applyOnDialogueEnded)
                    {
                        _pendingBinding = binding;
                    }
                    else
                    {
                        ApplyBinding(binding, "对话事件触发剧情 Flag");
                    }
                }
            }
        }

        private void HandleDialogueEnded(Data.DialogueData dialogueData)
        {
            if (_pendingBinding == null)
            {
                return;
            }

            ApplyBinding(_pendingBinding, "对话结束后触发剧情 Flag");
            _pendingBinding = null;
        }

        private void ApplyBinding(EventFlagBinding binding, string logPrefix)
        {
            if (binding == null || string.IsNullOrWhiteSpace(binding.flagId) || GameManager.Instance == null || GameManager.Instance.Flags == null)
            {
                return;
            }

            GameManager.Instance.Flags.SetFlag(new FlagId(binding.flagId), binding.flagValue);
            Debug.Log($"{logPrefix}: {binding.flagId} = {binding.flagValue}", this);
        }
    }
}
