using UnityEngine;
using BS.Gameplay.Interaction;

namespace BS.Gameplay.UI.WorldText.Test
{
    /// <summary>
    /// 测试或简单剧情用的世界浮动文本触发器。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class WorldFloatingTextTrigger : MonoBehaviour
    {
        [Header("触发配置")]
        [SerializeField] private Transform target;
        [SerializeField] [TextArea(2, 4)] private string message = "……";
        [SerializeField] private KeyCode triggerKey = KeyCode.T;
        [SerializeField] private bool triggerOnPlayerEnter = true;
        [SerializeField] private bool triggerOnlyOnce = true;

        [Header("显示配置")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.4f, 0f);
        [SerializeField] private Vector2 screenOffset = Vector2.zero;
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float visibleDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float floatDistance = 24f;

        private bool _hasTriggered;

        private void Reset()
        {
            if (target == null)
            {
                target = transform;
            }

            var triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(triggerKey))
            {
                return;
            }

            Trigger();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!triggerOnPlayerEnter)
            {
                return;
            }

            if (other.GetComponentInParent<PlayerInteractor>() == null)
            {
                return;
            }

            Trigger();
        }

        [ContextMenu("Trigger")]
        public void Trigger()
        {
            if (triggerOnlyOnce && _hasTriggered)
            {
                return;
            }

            if (WorldFloatingTextUI.Instance == null)
            {
                return;
            }

            WorldFloatingTextUI.Instance.ShowText(
                target != null ? target : transform,
                message,
                worldOffset,
                screenOffset,
                fadeInDuration,
                visibleDuration,
                fadeOutDuration,
                floatDistance);

            _hasTriggered = true;
        }
    }
}
