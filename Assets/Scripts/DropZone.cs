using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("目标碎片")]
    public string targetFragmentID; // 这里填入对应的谎言ID，如 "lie-1"
    
    [HideInInspector] public bool isFilled = false;
    private Image zoneImage;
    private Color normalColor = new Color(0, 0, 0, 0.02f);
    private Color hoverColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

    void Awake()
    {
        zoneImage = GetComponent<Image>();
        if(zoneImage != null) zoneImage.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isFilled && eventData.pointerDrag != null && zoneImage != null)
            zoneImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isFilled && zoneImage != null)
            zoneImage.color = normalColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isFilled) return;
        if (zoneImage != null) zoneImage.color = normalColor;

        if (eventData.pointerDrag != null)
        {
            DraggableFragment fragment = eventData.pointerDrag.GetComponent<DraggableFragment>();
            if (fragment != null)
            {
                // 交给 Manager 去校验业务逻辑
                DiaryPuzzleManager.Instance.ProcessDrop(fragment, this);
            }
        }
    }
}