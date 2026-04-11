using System;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using BS.Gameplay.Player;
using UnityEngine;

namespace BS.Gameplay.Dialogue
{
    /// <summary>
    /// 对话运行管理器。
    /// 负责开始对话、按句推进、广播 UI 事件、触发行内事件，并在对话期间临时锁定玩家控制。
    /// </summary>
    public sealed class DialogueManager : MonoBehaviour
    {
        [Header("推进配置")]
        [SerializeField] private KeyCode advanceKey = KeyCode.Space;
        [SerializeField] private bool allowMouseAdvance = true;

        private DialogueSession _session;
        private PlayerInputReader _lockedInputReader;
        private PlayerMotor2D _lockedMotor;
        private PlayerInteractor _lockedInteractor;
        private bool _waitForAdvanceRelease;

        public event Action<DialogueData> DialogueStarted;
        public event Action<DialogueData> DialogueEnded;
        public event Action<DialogueLine, int, int> LineChanged;
        public event Action<string, DialogueLine, DialogueData> LineEventTriggered;

        public bool IsPlaying => _session != null;
        public DialogueData CurrentDialogue => _session != null ? _session.DialogueData : null;
        public DialogueLine CurrentLine => _session != null ? _session.CurrentLine : null;

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            if (_waitForAdvanceRelease)
            {
                if (!IsAdvancePressed())
                {
                    _waitForAdvanceRelease = false;
                }

                return;
            }

            if (IsAdvancePressedThisFrame())
            {
                Advance();
            }
        }

        public bool StartDialogue(DialogueData dialogueData, PlayerInteractor interactor = null)
        {
            if (dialogueData == null || !dialogueData.IsValid)
            {
                Debug.LogWarning("尝试开始无效对白。", this);
                return false;
            }

            if (IsPlaying)
            {
                Debug.LogWarning("已有对白正在播放，忽略新的开始请求。", this);
                return false;
            }

            _session = new DialogueSession(dialogueData);
            LockPlayer(interactor);
            _waitForAdvanceRelease = true;

            DialogueStarted?.Invoke(dialogueData);
            Advance();
            return true;
        }

        public void Advance()
        {
            if (!IsPlaying)
            {
                return;
            }

            if (!_session.HasNextLine)
            {
                EndDialogue();
                return;
            }

            var line = _session.MoveNext();
            if (line == null)
            {
                EndDialogue();
                return;
            }

            LineChanged?.Invoke(line, _session.CurrentLineIndex, _session.DialogueData.Lines.Count);

            if (!string.IsNullOrWhiteSpace(line.EventKey))
            {
                LineEventTriggered?.Invoke(line.EventKey, line, _session.DialogueData);
            }
        }

        public void EndDialogue()
        {
            if (!IsPlaying)
            {
                return;
            }

            var endedDialogue = _session.DialogueData;
            _session = null;
            UnlockPlayer();

            DialogueEnded?.Invoke(endedDialogue);
        }

        private void LockPlayer(PlayerInteractor interactor)
        {
            _lockedInteractor = interactor != null
                ? interactor
                : FindFirstObjectByType<PlayerInteractor>(FindObjectsInactive.Exclude);

            if (_lockedInteractor != null)
            {
                _lockedInteractor.PushInteractionLock();
            }

            var playerRoot = _lockedInteractor != null ? _lockedInteractor.gameObject : null;
            if (playerRoot == null)
            {
                return;
            }

            _lockedInputReader = playerRoot.GetComponent<PlayerInputReader>();
            if (_lockedInputReader != null)
            {
                _lockedInputReader.PushInputLock();
            }

            _lockedMotor = playerRoot.GetComponent<PlayerMotor2D>();
            if (_lockedMotor != null)
            {
                _lockedMotor.PushMovementLock();
            }
        }

        private void UnlockPlayer()
        {
            if (_lockedMotor != null)
            {
                _lockedMotor.PopMovementLock();
                _lockedMotor = null;
            }

            if (_lockedInputReader != null)
            {
                _lockedInputReader.PopInputLock();
                _lockedInputReader = null;
            }

            if (_lockedInteractor != null)
            {
                _lockedInteractor.PopInteractionLock();
                _lockedInteractor = null;
            }
        }

        private bool IsAdvancePressedThisFrame()
        {
            return Input.GetKeyDown(advanceKey) || allowMouseAdvance && Input.GetMouseButtonDown(0);
        }

        private bool IsAdvancePressed()
        {
            return Input.GetKey(advanceKey) || allowMouseAdvance && Input.GetMouseButton(0);
        }
    }
}
