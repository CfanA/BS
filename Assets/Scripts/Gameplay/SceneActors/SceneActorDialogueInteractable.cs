using System.Collections;
using BS.Core;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using UnityEngine;

namespace BS.Gameplay.SceneActors
{
    public sealed class SceneActorDialogueInteractable : InteractableBase
    {
        [Header("Dialogue")]
        [SerializeField] private DialogueData preSequenceDialogue;

        [Header("Actor")]
        [SerializeField] private SceneActor actor;
        [SerializeField] private SceneActorSequenceAsset sequenceAsset;
        [SerializeField] private SceneActorSequence inlineSequence = new();
        [SerializeField] private bool useInlineSequence;
        [SerializeField] private bool useActorDefaultSequence = true;

        [Header("Trigger")]
        [SerializeField] private bool triggerOnce = true;

        private bool _hasTriggered;

        private void Awake()
        {
            if (actor == null)
            {
                actor = GetComponentInParent<SceneActor>();
            }
        }

        public override bool CanInteract(PlayerInteractor interactor)
        {
            if (!base.CanInteract(interactor))
            {
                return false;
            }

            if (triggerOnce && _hasTriggered)
            {
                return false;
            }

            if (actor == null || actor.IsPlaying || preSequenceDialogue == null)
            {
                return false;
            }

            var dialogueManager = GameManager.Instance != null ? GameManager.Instance.Dialogue : null;
            return dialogueManager == null || !dialogueManager.IsPlaying;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (preSequenceDialogue == null)
            {
                Debug.LogWarning("SceneActorDialogueInteractable 缺少 preSequenceDialogue。", this);
                return;
            }

            if (actor == null)
            {
                Debug.LogWarning("SceneActorDialogueInteractable 缺少 SceneActor。", this);
                return;
            }

            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Dialogue == null)
            {
                Debug.LogError("找不到 DialogueManager，无法播放交互对白。", this);
                return;
            }

            if (!gameManager.Dialogue.StartDialogue(preSequenceDialogue, interactor))
            {
                return;
            }

            _hasTriggered = true;
            StartCoroutine(WaitDialogueThenPlay(interactor));
        }

        private IEnumerator WaitDialogueThenPlay(PlayerInteractor interactor)
        {
            var dialogueManager = GameManager.Instance != null ? GameManager.Instance.Dialogue : null;
            if (dialogueManager == null)
            {
                yield break;
            }

            while (dialogueManager.IsPlaying)
            {
                yield return null;
            }

            if (sequenceAsset != null)
            {
                actor.PlaySequence(sequenceAsset, interactor);
                yield break;
            }

            if (useInlineSequence)
            {
                actor.PlaySequence(inlineSequence, interactor);
                yield break;
            }

            if (useActorDefaultSequence)
            {
                actor.PlayDefaultSequence(interactor);
            }
        }
    }
}
