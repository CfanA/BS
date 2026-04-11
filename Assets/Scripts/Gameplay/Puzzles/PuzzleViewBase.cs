using BS.Gameplay.Interaction;
using BS.Gameplay.Player;
using UnityEngine;

namespace BS.Gameplay.Puzzles
{
    /// <summary>
    /// 通用谜题视图基类。
    /// 负责视图打开关闭和玩家临时锁定，不负责具体判题。
    /// </summary>
    public abstract class PuzzleViewBase : MonoBehaviour
    {
        private PlayerInteractor _boundInteractor;
        private PlayerInputReader _boundInputReader;
        private PlayerMotor2D _boundMotor;

        public bool IsOpen { get; private set; }
        protected PuzzleBase BoundPuzzle { get; private set; }

        public virtual void Open(PuzzleBase puzzle, PlayerInteractor interactor)
        {
            BoundPuzzle = puzzle;
            BindPlayer(interactor);
            IsOpen = true;
        }

        public virtual void Close()
        {
            UnbindPlayer();
            BoundPuzzle = null;
            IsOpen = false;
        }

        private void BindPlayer(PlayerInteractor interactor)
        {
            _boundInteractor = interactor;
            if (_boundInteractor != null)
            {
                _boundInteractor.PushInteractionLock();

                _boundInputReader = _boundInteractor.GetComponent<PlayerInputReader>();
                if (_boundInputReader != null)
                {
                    _boundInputReader.PushInputLock();
                }

                _boundMotor = _boundInteractor.GetComponent<PlayerMotor2D>();
                if (_boundMotor != null)
                {
                    _boundMotor.PushMovementLock();
                }
            }
        }

        private void UnbindPlayer()
        {
            if (_boundMotor != null)
            {
                _boundMotor.PopMovementLock();
                _boundMotor = null;
            }

            if (_boundInputReader != null)
            {
                _boundInputReader.PopInputLock();
                _boundInputReader = null;
            }

            if (_boundInteractor != null)
            {
                _boundInteractor.PopInteractionLock();
                _boundInteractor = null;
            }
        }
    }
}
