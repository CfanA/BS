using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableFragment : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("碎片属性")]
    public string fragmentID; // 如 "lie-1", "truth-1"
    public bool isTruth;      // 勾选代表这是“真相”（放上去会报错）

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Vector3 originalPosition;
    
    private CanvasGroup canvasGroup;
    private Transform dragLayer; // 拖拽时的临时层级，保证显示在最前面

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // 自动寻找场景中的拖拽层（需要在Hierarchy建一个名为 Drag_Layer 的空节点）
        dragLayer = GameObject.Find("Drag_Layer")?.transform; 
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.position;

        // 移到顶层显示
        if (dragLayer != null) transform.SetParent(dragLayer);
        
        // 关键：拖拽时关闭射线的阻挡，这样底下的 DropZone 才能检测到鼠标
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // 如果没有被 DropZone 接收（没有改变父节点），则弹回原位
        if (transform.parent == dragLayer)
        {
            SnapBack();
        }
    }

    public void SnapBack()
    {
        transform.SetParent(originalParent);
        transform.position = originalPosition;
    }
}