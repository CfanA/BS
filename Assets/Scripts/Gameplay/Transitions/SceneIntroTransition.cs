using System.Collections;
using BS.Core;
using BS.Foundation.Ids;
using BS.Gameplay.Interaction;
using BS.Gameplay.Player;
using BS.Gameplay.Transitions.Data;
using UnityEngine;

namespace BS.Gameplay.Transitions
{
    /// <summary>
    /// 默认场景开场过场。
    /// Exit 只做最短遮罩接管，Enter 在遮罩下播放场景开场演出并最终 reveal。
    /// </summary>
    public sealed class SceneIntroTransition : ISceneTransition
    {
        private readonly SceneId _targetSceneId;
        private readonly SceneTransitionPresenter _presenter;
        private PlayerInputReader _playerInputReader;
        private PlayerMotor2D _playerMotor2D;
        private PlayerInteractor _playerInteractor;
        private bool _lockedInput;
        private bool _lockedMovement;
        private bool _lockedInteraction;

        public SceneIntroTransition(SceneId targetSceneId, SceneTransitionPresenter presenter)
        {
            _targetSceneId = targetSceneId;
            _presenter = presenter;
        }

        public IEnumerator PlayExit()
        {
            if (_presenter == null)
            {
                yield break;
            }

            LockPlayer(forceRefresh: true);
            _presenter.CoverWithBlack();
            yield return _presenter.FadeBlackTo(1f, 0.05f);
        }

        public IEnumerator PlayEnter()
        {
            if (_presenter == null)
            {
                yield break;
            }

            LockPlayer(forceRefresh: true);
            _presenter.CoverWithBlack();

            if (!_presenter.TryGetProfile(_targetSceneId.Value, out var profile))
            {
                Debug.LogWarning($"[SceneIntroTransition] 使用默认开场配置: {_targetSceneId.Value}");
                profile = new SceneIntroProfile
                {
                    SceneName = _targetSceneId.Value,
                    InitialBlackHold = 0.15f,
                    RevealDuration = 0.3f
                };
            }
            else
            {
                Debug.LogWarning($"[SceneIntroTransition] 命中开场配置: {_targetSceneId.Value}");
            }

            if (profile.InitialBlackHold > 0f)
            {
                yield return new WaitForSeconds(profile.InitialBlackHold);
            }

            yield return _presenter.ShowIntroText(profile);
            yield return _presenter.ShowTitle(profile);

            if (profile.EnableWhiteFlash)
            {
                yield return _presenter.FadeBlackTo(0f, 0f);
                yield return _presenter.FadeWhiteTo(1f, 0f);
                yield return _presenter.FadeWhiteTo(0f, profile.WhiteFlashDuration);
            }
            else
            {
                yield return _presenter.FadeBlackTo(0f, profile.RevealDuration);
            }

            _presenter.ResetState();
            _presenter.SetRootVisible(false);
            UnlockPlayer();
        }

        public static ISceneTransition CreateOrDefault(SceneId targetSceneId)
        {
            return SceneTransitionPresenter.Instance != null
                ? new SceneIntroTransition(targetSceneId, SceneTransitionPresenter.Instance)
                : new NoSceneTransition();
        }

        private void LockPlayer(bool forceRefresh)
        {
            TryBindPlayerReferences(forceRefresh);

            if (_playerInputReader != null && !_lockedInput)
            {
                _playerInputReader.PushInputLock();
                _lockedInput = true;
            }

            if (_playerMotor2D != null && !_lockedMovement)
            {
                _playerMotor2D.PushMovementLock();
                _lockedMovement = true;
            }

            if (_playerInteractor != null && !_lockedInteraction)
            {
                _playerInteractor.PushInteractionLock();
                _lockedInteraction = true;
            }
        }

        private void UnlockPlayer()
        {
            TryBindPlayerReferences(forceRefresh: true);

            if (_playerInteractor != null && _lockedInteraction)
            {
                _playerInteractor.PopInteractionLock();
                _lockedInteraction = false;
                _playerInteractor.SetInteractionEnabled(true);
            }

            if (_playerMotor2D != null && _lockedMovement)
            {
                _playerMotor2D.PopMovementLock();
                _lockedMovement = false;
                _playerMotor2D.SetMovementEnabled(true);
            }

            if (_playerInputReader != null && _lockedInput)
            {
                _playerInputReader.PopInputLock();
                _lockedInput = false;
                _playerInputReader.SetInputEnabled(true);
            }
        }

        private void TryBindPlayerReferences(bool forceRefresh)
        {
            if (forceRefresh || _playerInputReader == null)
            {
                _playerInputReader = Object.FindFirstObjectByType<PlayerInputReader>(FindObjectsInactive.Exclude);
            }

            if (forceRefresh || _playerMotor2D == null)
            {
                _playerMotor2D = Object.FindFirstObjectByType<PlayerMotor2D>(FindObjectsInactive.Exclude);
            }

            if (forceRefresh || _playerInteractor == null)
            {
                _playerInteractor = Object.FindFirstObjectByType<PlayerInteractor>(FindObjectsInactive.Exclude);
            }
        }
    }
}
