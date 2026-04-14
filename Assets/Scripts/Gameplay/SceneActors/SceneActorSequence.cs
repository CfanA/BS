using System;
using System.Collections.Generic;
using UnityEngine;

namespace BS.Gameplay.SceneActors
{
    [Serializable]
    public sealed class SceneActorSequence
    {
        [SerializeField] private string sequenceId;
        [SerializeField] private bool triggerOnce;
        [SerializeField] private bool ignoreIfAlreadyPlaying = true;
        [SerializeField] private bool lockPlayerInput = true;
        [SerializeField] private bool lockPlayerMovement = true;
        [SerializeField] private bool lockPlayerInteraction = true;
        [SerializeField] private bool disableActorInteractablesWhilePlaying = true;
        [SerializeField] private List<SceneActorStep> steps = new();

        public string SequenceId => sequenceId;
        public bool TriggerOnce => triggerOnce;
        public bool IgnoreIfAlreadyPlaying => ignoreIfAlreadyPlaying;
        public bool LockPlayerInput => lockPlayerInput;
        public bool LockPlayerMovement => lockPlayerMovement;
        public bool LockPlayerInteraction => lockPlayerInteraction;
        public bool DisableActorInteractablesWhilePlaying => disableActorInteractablesWhilePlaying;
        public IReadOnlyList<SceneActorStep> Steps => steps;
    }
}
