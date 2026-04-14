using BS.Core;
using BS.Gameplay.Interaction;
using BS.Gameplay.Story;
using UnityEngine;

namespace BS.Gameplay.SceneActors
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class SceneActorTrigger : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private SceneActor actor;
        [SerializeField] private SceneActorSequenceAsset sequenceAsset;
        [SerializeField] private SceneActorSequence inlineSequence = new();
        [SerializeField] private bool useInlineSequence;
        [SerializeField] private bool useActorDefaultSequence = true;

        [Header("Trigger")]
        [SerializeField] private bool triggerOnPlayerEnter = true;
        [SerializeField] private bool triggerOnce = true;

        [Header("After Complete")]
        [SerializeField] private FlagReference completedFlag;
        [SerializeField] private bool completedFlagValue = true;

        private bool _hasTriggered;

        private void Reset()
        {
            var triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        public bool TryTrigger(PlayerInteractor interactor = null)
        {
            if (triggerOnce && _hasTriggered)
            {
                return false;
            }

            if (actor == null)
            {
                actor = GetComponentInParent<SceneActor>();
            }

            if (actor == null)
            {
                Debug.LogWarning("SceneActorTrigger 缺少 SceneActor。", this);
                return false;
            }

            var started = sequenceAsset != null
                ? actor.PlaySequence(sequenceAsset, interactor, HandleCompleted)
                : useInlineSequence
                    ? actor.PlaySequence(inlineSequence, interactor, HandleCompleted)
                    : useActorDefaultSequence && actor.PlayDefaultSequence(interactor, HandleCompleted);

            if (started)
            {
                _hasTriggered = true;
            }

            return started;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!triggerOnPlayerEnter)
            {
                return;
            }

            var interactor = other.GetComponentInParent<PlayerInteractor>();
            if (interactor == null)
            {
                return;
            }

            TryTrigger(interactor);
        }

        private void HandleCompleted(SceneActor sceneActor)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Flags == null || !completedFlag.IsValid)
            {
                return;
            }

            gameManager.Flags.SetFlag(completedFlag.FlagId, completedFlagValue);
        }
    }
}
