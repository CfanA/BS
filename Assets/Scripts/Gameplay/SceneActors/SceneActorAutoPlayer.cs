using UnityEngine;

namespace BS.Gameplay.SceneActors
{
    public sealed class SceneActorAutoPlayer : MonoBehaviour
    {
        [SerializeField] private SceneActor actor;
        [SerializeField] private SceneActorSequenceAsset sequenceAsset;
        [SerializeField] private SceneActorSequence inlineSequence = new();
        [SerializeField] private bool useInlineSequence;
        [SerializeField] private bool useActorDefaultSequence = true;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool playOnlyOnce = true;

        private bool _hasPlayed;

        private void Awake()
        {
            if (actor == null)
            {
                actor = GetComponentInParent<SceneActor>();
            }
        }

        private void Start()
        {
            if (!playOnStart)
            {
                return;
            }

            TryPlay();
        }

        public bool TryPlay()
        {
            if (playOnlyOnce && _hasPlayed)
            {
                return false;
            }

            if (actor == null)
            {
                Debug.LogWarning("SceneActorAutoPlayer 缺少 SceneActor。", this);
                return false;
            }

            var started = sequenceAsset != null
                ? actor.PlaySequence(sequenceAsset)
                : useInlineSequence
                    ? actor.PlaySequence(inlineSequence)
                    : useActorDefaultSequence && actor.PlayDefaultSequence();

            if (started)
            {
                _hasPlayed = true;
            }

            return started;
        }
    }
}
