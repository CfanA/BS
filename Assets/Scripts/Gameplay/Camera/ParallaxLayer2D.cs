using UnityEngine;

namespace BS.Gameplay.CameraSystem
{
    /// <summary>
    /// 2D 视差层。
    /// 挂在场景中的前景或背景物体上，根据相机位移做相对偏移。
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class ParallaxLayer2D : MonoBehaviour
    {
        [Header("相机引用")]
        [SerializeField] private Transform targetCamera;

        [Header("视差参数")]
        [SerializeField] private Vector2 parallaxMultiplier = new Vector2(1.1f, 0f);
        [SerializeField] private bool useInitialZ = true;

        private Vector3 _initialLayerPosition;
        private Vector3 _initialCameraPosition;

        private void Awake()
        {
            CacheCameraIfNeeded();
            CaptureInitialState();
        }

        private void OnEnable()
        {
            CacheCameraIfNeeded();
            CaptureInitialState();
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                return;
            }

            var cameraOffset = targetCamera.position - _initialCameraPosition;
            var nextPosition = _initialLayerPosition;

            nextPosition.x += cameraOffset.x * parallaxMultiplier.x;
            nextPosition.y += cameraOffset.y * parallaxMultiplier.y;

            if (!useInitialZ)
            {
                nextPosition.z = transform.position.z;
            }

            transform.position = nextPosition;
        }

        /// <summary>
        /// 运行时切换视差跟随相机。
        /// </summary>
        public void SetTargetCamera(Transform newTargetCamera, bool recaptureState = true)
        {
            targetCamera = newTargetCamera;

            if (recaptureState)
            {
                CaptureInitialState();
            }
        }

        /// <summary>
        /// 运行时重置当前视差基准点。
        /// </summary>
        public void CaptureInitialState()
        {
            _initialLayerPosition = transform.position;
            _initialCameraPosition = targetCamera != null ? targetCamera.position : Vector3.zero;
        }

        private void CacheCameraIfNeeded()
        {
            if (targetCamera != null)
            {
                return;
            }

            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                targetCamera = mainCamera.transform;
            }
        }
    }
}
