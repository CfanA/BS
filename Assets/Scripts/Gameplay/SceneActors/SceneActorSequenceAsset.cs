using UnityEngine;

namespace BS.Gameplay.SceneActors
{
    [CreateAssetMenu(
        fileName = "SceneActorSequence",
        menuName = "BS/Story/Scene Actor Sequence")]
    public sealed class SceneActorSequenceAsset : ScriptableObject
    {
        [SerializeField] private SceneActorSequence sequence = new();

        public SceneActorSequence Sequence => sequence;
    }
}
