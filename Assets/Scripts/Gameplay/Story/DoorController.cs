using UnityEngine;

namespace BS.Gameplay.Story
{
    /// <summary>
    /// 门的最小控制器。
    /// 当前只处理可见性、阻挡碰撞和交互禁用。
    /// </summary>
    public sealed class DoorController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GameObject closedVisual;
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private MonoBehaviour interactableToDisable;

        public bool IsOpen { get; private set; }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            IsOpen = true;

            if (closedVisual != null)
            {
                closedVisual.SetActive(false);
            }

            if (blockingCollider != null)
            {
                blockingCollider.enabled = false;
            }

            if (interactableToDisable != null)
            {
                interactableToDisable.enabled = false;
            }
        }
    }
}
