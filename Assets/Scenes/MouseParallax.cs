using UnityEngine;

public class MouseParallax : MonoBehaviour
{
    [Header("视差配置")]
    [Tooltip("移动幅度。背景用正数(如30)，UI用负数(如-15)产生纵深")]
    public float moveAmount = 30f; 
    [Tooltip("插值平滑度，数值越小跟随越紧密")]
    public float smoothTime = 1.5f;

    private Vector3 initialPosition;
    private Vector2 currentVelocity;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // 获取鼠标在屏幕上的相对位置 (-0.5 到 0.5)
        float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
        float mouseY = (Input.mousePosition.y / Screen.height) - 0.5f;

        // 计算目标位置（相反方向位移）
        Vector3 targetPosition = new Vector3(
            initialPosition.x - (mouseX * moveAmount),
            initialPosition.y - (mouseY * moveAmount),
            initialPosition.z
        );

        // 使用平滑阻尼函数
        float newX = Mathf.SmoothDamp(transform.localPosition.x, targetPosition.x, ref currentVelocity.x, smoothTime);
        float newY = Mathf.SmoothDamp(transform.localPosition.y, targetPosition.y, ref currentVelocity.y, smoothTime);

        transform.localPosition = new Vector3(newX, newY, transform.localPosition.z);
    }
}