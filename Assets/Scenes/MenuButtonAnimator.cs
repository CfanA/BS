using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // 引入场景管理
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class MenuButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum ActionType { Start, ShowMessage, Quit }

    [Header("交互配置")]
    public ActionType actionType = ActionType.ShowMessage;
    public string targetSceneName = "第一章_场景0_主角房间_周日上午";
    public TMP_Text systemMsgBox; // 用于显示"还未开发"

    [Header("入场配置")]
    public float introDelay = 1.5f; // 依次设为 1.5, 1.8, 2.1 等

    [Header("UI 引用")]
    public TMP_Text buttonText;
    public Image indicatorDot; // 按钮左侧的小圆点

    private float originalSpacing;
    private Vector3 originalPosition;
    private Color normalColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
    private Color hoverColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

    void Start()
    {
        originalSpacing = buttonText.characterSpacing;
        originalPosition = transform.localPosition;

        // 初始状态隐藏
        buttonText.alpha = 0f;
        buttonText.color = normalColor;
        transform.localPosition = originalPosition - new Vector3(15f, 0, 0); // 往左偏移
        if (indicatorDot != null) indicatorDot.color = new Color(1, 1, 1, 0);

        // 延迟入场动画：淡入 + 缓慢右移 (极其克制的 2秒 缓冲)
        Sequence introSeq = DOTween.Sequence();
        introSeq.AppendInterval(introDelay);
        introSeq.Append(buttonText.DOFade(0.7f, 2f).SetEase(Ease.OutCubic));
        introSeq.Join(transform.DOLocalMoveX(originalPosition.x, 2f).SetEase(Ease.OutCubic));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.DOKill(false); // 停止颜色动画，避免冲突
        buttonText.DOColor(hoverColor, 0.8f);
        
        // 字间距拉开，增加虚无感
        DOTween.To(() => buttonText.characterSpacing, x => buttonText.characterSpacing = x, originalSpacing + 5f, 0.8f);
        transform.DOLocalMoveX(originalPosition.x + 10f, 0.8f).SetEase(Ease.OutCubic);
        
        if (indicatorDot != null) indicatorDot.DOFade(1f, 0.8f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.DOKill(false);
        buttonText.DOColor(normalColor, 0.8f);
        
        DOTween.To(() => buttonText.characterSpacing, x => buttonText.characterSpacing = x, originalSpacing, 0.8f);
        transform.DOLocalMoveX(originalPosition.x, 0.8f).SetEase(Ease.OutCubic);
        
        if (indicatorDot != null) indicatorDot.DOFade(0f, 0.8f);
    }

    // 处理点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        // 点击时的极轻微闪烁反馈 (闪白再恢复)
        buttonText.DOColor(Color.white, 0.1f).OnComplete(() => buttonText.DOColor(hoverColor, 0.2f));

        switch (actionType)
        {
            case ActionType.Start:
                Debug.Log($"[系统] 准备加载场景: {targetSceneName}");
                // TODO: 可以在这里添加一个 AudioManager 音效 和 黑幕淡出
                SceneManager.LoadScene(targetSceneName);
                break;

            case ActionType.ShowMessage:
                ShowSystemMessage("（该功能还未开发）");
                break;

            case ActionType.Quit:
                Debug.Log("[系统] 退出游戏");
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
                break;
        }
    }

    // 在屏幕以同样忧郁的风格显示文字
    private void ShowSystemMessage(string msg)
    {
        if (systemMsgBox == null) return;
        
        // 清除上一个正在播放的动画
        systemMsgBox.DOKill();
        
        systemMsgBox.text = msg;
        systemMsgBox.color = new Color(0.6f, 0.6f, 0.6f, 0f); // 确保从透明开始
        
        Sequence seq = DOTween.Sequence();
        seq.Append(systemMsgBox.DOFade(0.8f, 0.6f).SetEase(Ease.OutQuad)); // 缓慢浮现
        seq.AppendInterval(2.0f); // 停留2秒
        seq.Append(systemMsgBox.DOFade(0f, 1.2f).SetEase(Ease.InQuad));   // 更缓慢地消散
    }
}