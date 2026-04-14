using System;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Story;
using UnityEngine;

namespace BS.Gameplay.SceneActors
{
    public enum SceneActorStepType
    {
        Wait,
        MoveToPoint,
        FaceDirection,
        FaceTarget,
        SetActorVisible,
        TeleportToPoint,
        PlayDialogue,
        SetFlag,
        TriggerGameObject,
        EndSequence
    }

    public enum SceneActorFacingDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    [Serializable]
    public sealed class SceneActorStep
    {
        [SerializeField] private SceneActorStepType stepType;

        [Header("Wait")]
        [SerializeField] private float waitSeconds = 0.5f;

        [Header("Move / Teleport")]
        [SerializeField] private Transform point;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float arriveDistance = 0.02f;

        [Header("Facing")]
        [SerializeField] private SceneActorFacingDirection facingDirection = SceneActorFacingDirection.Right;
        [SerializeField] private Transform target;

        [Header("Visibility")]
        [SerializeField] private bool visible = true;

        [Header("Dialogue")]
        [SerializeField] private DialogueData dialogueData;

        [Header("Flag")]
        [SerializeField] private FlagReference flag;
        [SerializeField] private bool flagValue = true;

        [Header("GameObject")]
        [SerializeField] private GameObject targetObject;
        [SerializeField] private bool activeState = true;

        public SceneActorStepType StepType => stepType;
        public float WaitSeconds => waitSeconds;
        public Transform Point => point;
        public float MoveSpeed => moveSpeed;
        public float ArriveDistance => Mathf.Max(0.001f, arriveDistance);
        public SceneActorFacingDirection FacingDirection => facingDirection;
        public Transform Target => target;
        public bool Visible => visible;
        public DialogueData DialogueData => dialogueData;
        public FlagReference Flag => flag;
        public bool FlagValue => flagValue;
        public GameObject TargetObject => targetObject;
        public bool ActiveState => activeState;
    }
}
