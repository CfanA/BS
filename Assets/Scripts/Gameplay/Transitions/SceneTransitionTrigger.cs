using BS.Gameplay.Transitions.Data;
using UnityEngine;

namespace BS.Gameplay.Transitions
{
    /// <summary>
    /// 简单的过场触发器。
    /// 可挂在 Trigger、调查物或临时测试对象上。
    /// </summary>
    public sealed class SceneTransitionTrigger : MonoBehaviour
    {
        [Header("过场配置")]
        [SerializeField] private SceneTransitionPreset preset;
        [SerializeField] private string targetSceneName;
        [SerializeField] private string targetSpawnPointId;

        [Header("触发配置")]
        [SerializeField] private bool triggerOnStart;
        [SerializeField] private bool triggerOnlyOnce = true;

        private bool _hasTriggered;

        private void Start()
        {
            if (triggerOnStart)
            {
                Trigger();
            }
        }

        [ContextMenu("Trigger")]
        public void Trigger()
        {
            if (triggerOnlyOnce && _hasTriggered)
            {
                return;
            }

            if (SceneTransitionController.Instance == null || preset == null)
            {
                return;
            }

            var request = string.IsNullOrWhiteSpace(targetSceneName)
                ? new SceneTransitionRequest()
                : new SceneTransitionRequest(targetSceneName, targetSpawnPointId);

            if (SceneTransitionController.Instance.Play(preset, request))
            {
                _hasTriggered = true;
            }
        }
    }
}
