using UnityEngine;

namespace BS.Gameplay.Interaction
{
    /// <summary>
    /// 统一交互协议。
    /// 所有可交互对象都通过这套接口向玩家暴露能力。
    /// </summary>
    public interface IInteractable
    {
        string DisplayName { get; }
        string PromptText { get; }
        int Priority { get; }
        bool CanInteract(PlayerInteractor interactor);
        Vector2 GetInteractionPoint();
        void OnEnterRange(PlayerInteractor interactor);
        void OnExitRange(PlayerInteractor interactor);
        void Interact(PlayerInteractor interactor);
    }
}
