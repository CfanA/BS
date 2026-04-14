using BS.Gameplay.Interaction;
using UnityEngine;

namespace BS.Gameplay.SceneActors
{
    public sealed class SceneActorSequenceInteractable : InteractableBase
    {
        [Header("Actor")]
        [SerializeField] private SceneActor actor;
        [SerializeField] private SceneActorSequenceAsset sequenceAsset;
        [SerializeField] private SceneActorSequence inlineSequence = new();
        [SerializeField] private bool useInlineSequence;
        [SerializeField] private bool useActorDefaultSequence = true;
        [SerializeField] private bool blockWhileActorPlaying = true;

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

            if (actor == null)
            {
                return false;
            }

            if (blockWhileActorPlaying && actor.IsPlaying)
            {
                return false;
            }

            return sequenceAsset != null || useInlineSequence || useActorDefaultSequence;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (actor == null)
            {
                Debug.LogWarning("SceneActorSequenceInteractable 缺少 SceneActor。", this);
                return;
            }

            if (sequenceAsset != null)
            {
                actor.PlaySequence(sequenceAsset, interactor);
                return;
            }

            if (useInlineSequence)
            {
                actor.PlaySequence(inlineSequence, interactor);
                return;
            }

            if (useActorDefaultSequence)
            {
                actor.PlayDefaultSequence(interactor);
            }
        }
    }
}
