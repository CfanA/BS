using UnityEngine;

namespace BS.Gameplay.Interaction.Test
{
    /// <summary>
    /// 用于测试“拾取物体”流程的基础对象。
    /// 当前版本先输出日志并隐藏自身，后续可接入背包和存档。
    /// </summary>
    public sealed class PickupObject : InteractableBase
    {
        [Header("拾取配置")]
        [SerializeField] private string itemId = "sample_item";
        [SerializeField] private bool hideAfterPickup = true;

        private bool _pickedUp;

        public override bool CanInteract(PlayerInteractor interactor)
        {
            return !_pickedUp && base.CanInteract(interactor);
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (_pickedUp)
            {
                return;
            }

            _pickedUp = true;
            Debug.Log($"拾取物体: {DisplayName} | ItemId: {itemId}", this);

            if (hideAfterPickup)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
