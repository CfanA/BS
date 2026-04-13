using UnityEngine;

namespace BS.Gameplay.CameraSystem
{
    /// <summary>
    /// 2D 相机跟随与范围限制。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [Header("跟随目标")]
        [SerializeField] private Transform target;

        [Header("边界")]
        [SerializeField] private BoxCollider2D boundsCollider;

        [Header("跟随参数")]
        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY = true;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private bool smoothFollow = true;
        [SerializeField] private float smoothTime = 0.15f;

        private Camera _camera;
        private Vector3 _currentVelocity;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            if (_camera == null)
            {
                Debug.LogError("CameraFollow2D 需要挂在 Camera 上。", this);
            }
        }

        private void LateUpdate()
        {
            if (_camera == null || target == null)
            {
                return;
            }

            var desiredPosition = transform.position;

            if (followX)
            {
                desiredPosition.x = target.position.x + offset.x;
            }

            if (followY)
            {
                desiredPosition.y = target.position.y + offset.y;
            }

            desiredPosition.z = offset.z;

            desiredPosition = ClampToBounds(desiredPosition);

            if (smoothFollow)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref _currentVelocity,
                    smoothTime);
            }
            else
            {
                transform.position = desiredPosition;
            }
        }

        /// <summary>
        /// 在运行时切换跟随目标。
        /// </summary>
        public void SetTarget(Transform newTarget, bool snapImmediately = false)
        {
            target = newTarget;

            if (snapImmediately && target != null)
            {
                var snappedPosition = transform.position;

                if (followX)
                {
                    snappedPosition.x = target.position.x + offset.x;
                }

                if (followY)
                {
                    snappedPosition.y = target.position.y + offset.y;
                }

                snappedPosition.z = offset.z;
                transform.position = ClampToBounds(snappedPosition);
                _currentVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// 在运行时切换边界。
        /// </summary>
        public void SetBounds(BoxCollider2D newBoundsCollider, bool snapImmediately = false)
        {
            boundsCollider = newBoundsCollider;

            if (snapImmediately)
            {
                var clamped = ClampToBounds(transform.position);
                transform.position = clamped;
                _currentVelocity = Vector3.zero;
            }
        }

        private Vector3 ClampToBounds(Vector3 desiredPosition)
        {
            if (boundsCollider == null || !_camera.orthographic)
            {
                return desiredPosition;
            }

            var bounds = boundsCollider.bounds;

            var halfHeight = _camera.orthographicSize;
            var halfWidth = halfHeight * _camera.aspect;

            var minX = bounds.min.x + halfWidth;
            var maxX = bounds.max.x - halfWidth;
            var minY = bounds.min.y + halfHeight;
            var maxY = bounds.max.y - halfHeight;

            // 如果边界比相机视野还小，则直接锁到边界中心，避免抖动和反向 clamp。
            if (minX > maxX)
            {
                desiredPosition.x = bounds.center.x;
            }
            else
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            }

            if (minY > maxY)
            {
                desiredPosition.y = bounds.center.y;
            }
            else
            {
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }

            return desiredPosition;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (boundsCollider == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(boundsCollider.bounds.center, boundsCollider.bounds.size);
        }
#endif
    }
}