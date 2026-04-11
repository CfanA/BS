using UnityEngine;

namespace BS.Gameplay.Interaction.Test
{
    /// <summary>
    /// 用于测试“查看物体”流程的基础对象。
    /// 当前版本先输出日志，后续可接入对话框、详情面板或剧情事件。
    /// </summary>
    public sealed class InspectableObject : InteractableBase
    {
        [Header("查看配置")]
        [SerializeField] private string inspectMessage = "这里似乎藏着一些信息。";

        public override void Interact(PlayerInteractor interactor)
        {
            Debug.Log($"查看物体: {DisplayName} | {inspectMessage}", this);
        }
    }
}
