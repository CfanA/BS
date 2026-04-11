using UnityEngine;

namespace BS.Gameplay.Interaction
{
    /// <summary>
    /// 可交互对象基类。
    /// 提供统一的名称、提示文本、优先级和范围回调，子类只关心自己的交互行为。
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("基础配置")]
        [SerializeField] private string displayName = "物体";
        [SerializeField] private string promptText = "交互";
        [SerializeField] private int priority;
        [SerializeField] private Transform interactionPoint;
        [SerializeField] private bool canInteract = true;

        public string DisplayName => displayName;
        public string PromptText => promptText;
        public int Priority => priority;

        public virtual bool CanInteract(PlayerInteractor interactor)
        {
            return canInteract && isActiveAndEnabled;
        }

        public virtual Vector2 GetInteractionPoint()
        {
            return interactionPoint != null ? interactionPoint.position : transform.position;
        }

        public virtual void OnEnterRange(PlayerInteractor interactor)
        {
        }

        public virtual void OnExitRange(PlayerInteractor interactor)
        {
        }

        public abstract void Interact(PlayerInteractor interactor);

        /// <summary>
        /// 供子类或剧情逻辑切换可交互状态。
        /// </summary>
        protected void SetInteractable(bool value)
        {
            canInteract = value;
        }
    }
}
