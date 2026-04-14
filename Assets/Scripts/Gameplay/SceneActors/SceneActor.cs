using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BS.Core;
using BS.Gameplay.Dialogue;
using BS.Gameplay.Dialogue.Data;
using BS.Gameplay.Interaction;
using BS.Gameplay.Player;
using UnityEngine;

namespace BS.Gameplay.SceneActors
{
    public sealed class SceneActor : MonoBehaviour
    {
        [Header("Default Sequence")]
        [SerializeField] private SceneActorSequence defaultSequence = new();

        [Header("Actor Presentation")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform facingRoot;
        [SerializeField] private SpriteRenderer facingSprite;
        [SerializeField] private Collider2D[] collidersToToggleWithVisibility;

        [Header("Busy State")]
        [SerializeField] private Behaviour[] interactablesToDisableWhilePlaying;

        private readonly HashSet<string> _playedSequenceKeys = new();
        private Coroutine _sequenceCoroutine;
        private PlayerInteractor _lockedInteractor;
        private PlayerInputReader _lockedInputReader;
        private PlayerMotor2D _lockedMotor;
        private SceneActorSequence _currentSequence;
        private Action<SceneActor> _onSequenceCompleted;
        private Vector2 _currentFacing = Vector2.right;

        public event Action<SceneActor> SequenceStarted;
        public event Action<SceneActor> SequenceCompleted;

        public bool IsPlaying => _sequenceCoroutine != null;
        public Vector2 CurrentFacing => _currentFacing;

        private void Awake()
        {
            if (facingRoot == null)
            {
                facingRoot = transform;
            }

            if (facingSprite == null)
            {
                facingSprite = GetComponentInChildren<SpriteRenderer>(true);
            }

            if (collidersToToggleWithVisibility == null || collidersToToggleWithVisibility.Length == 0)
            {
                collidersToToggleWithVisibility = GetComponentsInChildren<Collider2D>(true);
            }
        }

        public bool PlayDefaultSequence(PlayerInteractor interactor = null, Action<SceneActor> onCompleted = null)
        {
            return PlaySequence(defaultSequence, interactor, onCompleted);
        }

        public bool PlaySequence(SceneActorSequenceAsset asset, PlayerInteractor interactor = null, Action<SceneActor> onCompleted = null)
        {
            if (asset == null)
            {
                Debug.LogWarning("SceneActor 缺少 SceneActorSequenceAsset。", this);
                return false;
            }

            return PlaySequence(asset.Sequence, interactor, onCompleted, asset);
        }

        public bool PlaySequence(SceneActorSequence sequence, PlayerInteractor interactor = null, Action<SceneActor> onCompleted = null)
        {
            return PlaySequence(sequence, interactor, onCompleted, null);
        }

        public void StopSequence()
        {
            if (_sequenceCoroutine == null)
            {
                return;
            }

            StopCoroutine(_sequenceCoroutine);
            _sequenceCoroutine = null;
            RestoreAfterSequence();
            _currentSequence = null;
            CompleteSequence();
        }

        public void SetVisible(bool visible)
        {
            if (visualRoot != null && visualRoot != transform)
            {
                visualRoot.gameObject.SetActive(visible);
            }
            else
            {
                var renderers = GetComponentsInChildren<Renderer>(true);
                for (var i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = visible;
                }
            }

            if (collidersToToggleWithVisibility == null)
            {
                return;
            }

            for (var i = 0; i < collidersToToggleWithVisibility.Length; i++)
            {
                if (collidersToToggleWithVisibility[i] != null)
                {
                    collidersToToggleWithVisibility[i].enabled = visible;
                }
            }
        }

        public void FaceDirection(SceneActorFacingDirection direction)
        {
            switch (direction)
            {
                case SceneActorFacingDirection.Left:
                    ApplyFacing(Vector2.left);
                    break;
                case SceneActorFacingDirection.Right:
                    ApplyFacing(Vector2.right);
                    break;
                case SceneActorFacingDirection.Up:
                    ApplyFacing(Vector2.up);
                    break;
                case SceneActorFacingDirection.Down:
                    ApplyFacing(Vector2.down);
                    break;
            }
        }

        public void FaceTarget(Transform target)
        {
            if (target == null)
            {
                return;
            }

            var delta = target.position - transform.position;
            if (delta.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var facing = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                ? (delta.x >= 0f ? Vector2.right : Vector2.left)
                : (delta.y >= 0f ? Vector2.up : Vector2.down);

            ApplyFacing(facing);
        }

        private bool PlaySequence(
            SceneActorSequence sequence,
            PlayerInteractor interactor,
            Action<SceneActor> onCompleted,
            SceneActorSequenceAsset asset)
        {
            if (sequence == null)
            {
                Debug.LogWarning("SceneActor 缺少 SceneActorSequence。", this);
                return false;
            }

            if (IsPlaying)
            {
                if (sequence.IgnoreIfAlreadyPlaying)
                {
                    return false;
                }

                StopSequence();
            }

            var sequenceKey = BuildSequenceKey(sequence, asset);
            if (sequence.TriggerOnce && _playedSequenceKeys.Contains(sequenceKey))
            {
                return false;
            }

            _currentSequence = sequence;
            _onSequenceCompleted = onCompleted;
            _sequenceCoroutine = StartCoroutine(RunSequence(sequence, sequenceKey, interactor));
            return true;
        }

        private IEnumerator RunSequence(SceneActorSequence sequence, string sequenceKey, PlayerInteractor interactor)
        {
            SequenceStarted?.Invoke(this);
            SetActorInteractablesEnabled(!sequence.DisableActorInteractablesWhilePlaying);
            LockPlayer(sequence, interactor);

            var steps = sequence.Steps;
            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step == null)
                {
                    continue;
                }

                if (step.StepType == SceneActorStepType.EndSequence)
                {
                    break;
                }

                yield return ExecuteStep(step);
            }

            if (sequence.TriggerOnce)
            {
                _playedSequenceKeys.Add(sequenceKey);
            }

            _sequenceCoroutine = null;
            RestoreAfterSequence();
            _currentSequence = null;
            CompleteSequence();
        }

        private IEnumerator ExecuteStep(SceneActorStep step)
        {
            switch (step.StepType)
            {
                case SceneActorStepType.Wait:
                    if (step.WaitSeconds > 0f)
                    {
                        yield return new WaitForSeconds(step.WaitSeconds);
                    }
                    break;

                case SceneActorStepType.MoveToPoint:
                    yield return MoveToPoint(step.Point, step.MoveSpeed, step.ArriveDistance);
                    break;

                case SceneActorStepType.FaceDirection:
                    FaceDirection(step.FacingDirection);
                    break;

                case SceneActorStepType.FaceTarget:
                    FaceTarget(step.Target);
                    break;

                case SceneActorStepType.SetActorVisible:
                    SetVisible(step.Visible);
                    break;

                case SceneActorStepType.TeleportToPoint:
                    if (step.Point != null)
                    {
                        transform.position = step.Point.position;
                    }
                    break;

                case SceneActorStepType.PlayDialogue:
                    yield return PlayDialogue(step.DialogueData);
                    break;

                case SceneActorStepType.SetFlag:
                    SetFlag(step);
                    break;

                case SceneActorStepType.TriggerGameObject:
                    if (step.TargetObject != null)
                    {
                        step.TargetObject.SetActive(step.ActiveState);
                    }
                    break;
            }
        }

        private IEnumerator MoveToPoint(Transform point, float moveSpeed, float arriveDistance)
        {
            if (point == null)
            {
                yield break;
            }

            var speed = Mathf.Max(0.01f, moveSpeed);
            while (true)
            {
                var targetPosition = point.position;
                var currentPosition = transform.position;
                var delta = targetPosition - currentPosition;

                if (delta.sqrMagnitude <= arriveDistance * arriveDistance)
                {
                    transform.position = targetPosition;
                    yield break;
                }

                var direction = ((Vector2)delta).normalized;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    ApplyFacing(direction);
                }

                transform.position = Vector3.MoveTowards(
                    currentPosition,
                    targetPosition,
                    speed * Time.deltaTime);

                yield return null;
            }
        }

        private IEnumerator PlayDialogue(DialogueData dialogueData)
        {
            if (dialogueData == null)
            {
                yield break;
            }

            var dialogueManager = GameManager.Instance != null ? GameManager.Instance.Dialogue : null;
            if (dialogueManager == null)
            {
                Debug.LogError("找不到 DialogueManager，无法播放 SceneActor 对白。", this);
                yield break;
            }

            while (dialogueManager.IsPlaying)
            {
                yield return null;
            }

            if (!dialogueManager.StartDialogue(dialogueData, _lockedInteractor))
            {
                yield break;
            }

            while (dialogueManager.IsPlaying)
            {
                yield return null;
            }
        }

        private void SetFlag(SceneActorStep step)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Flags == null || !step.Flag.IsValid)
            {
                return;
            }

            gameManager.Flags.SetFlag(step.Flag.FlagId, step.FlagValue);
        }

        private void LockPlayer(SceneActorSequence sequence, PlayerInteractor interactor)
        {
            var actorInteractor = interactor != null
                ? interactor
                : FindFirstObjectByType<PlayerInteractor>(FindObjectsInactive.Exclude);

            if (sequence.LockPlayerInteraction && actorInteractor != null)
            {
                _lockedInteractor = actorInteractor;
                _lockedInteractor.PushInteractionLock();
            }
            else
            {
                _lockedInteractor = actorInteractor;
            }

            var playerRoot = actorInteractor != null ? actorInteractor.gameObject : null;
            if (playerRoot == null)
            {
                return;
            }

            if (sequence.LockPlayerInput)
            {
                _lockedInputReader = playerRoot.GetComponent<PlayerInputReader>();
                if (_lockedInputReader != null)
                {
                    _lockedInputReader.PushInputLock();
                }
            }

            if (sequence.LockPlayerMovement)
            {
                _lockedMotor = playerRoot.GetComponent<PlayerMotor2D>();
                if (_lockedMotor != null)
                {
                    _lockedMotor.PushMovementLock();
                }
            }
        }

        private void RestoreAfterSequence()
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

            if (_currentSequence == null || _currentSequence.DisableActorInteractablesWhilePlaying)
            {
                SetActorInteractablesEnabled(true);
            }

            if (_lockedInteractor != null)
            {
                if (_currentSequence == null || _currentSequence.LockPlayerInteraction)
                {
                    _lockedInteractor.PopInteractionLock();
                }

                _lockedInteractor = null;
            }
        }

        private void CompleteSequence()
        {
            SequenceCompleted?.Invoke(this);
            _onSequenceCompleted?.Invoke(this);
            _onSequenceCompleted = null;
        }

        private void ApplyFacing(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            _currentFacing = direction.normalized;

            if (Mathf.Abs(_currentFacing.x) <= 0.01f)
            {
                return;
            }

            if (facingSprite != null)
            {
                facingSprite.flipX = _currentFacing.x < 0f;
                return;
            }

            var root = facingRoot != null ? facingRoot : transform;
            var scale = root.localScale;
            scale.x = Mathf.Abs(scale.x) * (_currentFacing.x < 0f ? -1f : 1f);
            root.localScale = scale;
        }

        private void SetActorInteractablesEnabled(bool enabled)
        {
            if (interactablesToDisableWhilePlaying == null)
            {
                return;
            }

            for (var i = 0; i < interactablesToDisableWhilePlaying.Length; i++)
            {
                if (interactablesToDisableWhilePlaying[i] != null)
                {
                    interactablesToDisableWhilePlaying[i].enabled = enabled;
                }
            }
        }

        private static string BuildSequenceKey(SceneActorSequence sequence, SceneActorSequenceAsset asset)
        {
            if (asset != null)
            {
                return $"asset:{asset.GetInstanceID()}";
            }

            if (!string.IsNullOrWhiteSpace(sequence.SequenceId))
            {
                return $"id:{sequence.SequenceId}";
            }

            return $"runtime:{RuntimeHelpers.GetHashCode(sequence)}";
        }
    }
}
