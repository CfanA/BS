using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

[System.Serializable]
public struct PlantPageOption
{
    public Button btnTellHim; 
    public bool isCorrect;    
    [TextArea(2, 4)] 
    public string feedback;   
}

public class PlantGuideManager : MonoBehaviour
{
    [Header("全局引用")]
    public CanvasGroup mainCanvasGroup; // 拖入整个小游戏的根节点 (Canvas_PlantGuide)

    [Header("页面配置")]
    public CanvasGroup[] pages; 
    public TMP_Text pageIndicatorText;
    public Button btnPrev;
    public Button btnNext;

    [Header("选项配置")]
    public List<PlantPageOption> pageOptions = new List<PlantPageOption>();

    [Header("反馈对话框")]
    public CanvasGroup dialogueToast;
    public TMP_Text dialogueText;

    [Header("结局遮罩")]
    public CanvasGroup fragmentOverlay;
    public Button btnCloseGame; // 必须在 Inspector 中拖入“合上图鉴”按钮

    private int currentPage = 0;
    private Tween currentDialogueTween;

    void Start()
    {
        // --- 1. 引用检查 ---
        if (pages == null || pages.Length == 0 || btnPrev == null || btnNext == null)
        {
            Debug.LogError("【配置错误】Pages、Btn Prev 或 Btn Next 未赋值。");
            return;
        }

        // --- 2. 状态初始化 ---
        for (int i = 0; i < pages.Length; i++)
        {
            bool isFirst = (i == 0);
            pages[i].alpha = isFirst ? 1f : 0f;
            pages[i].interactable = isFirst;
            pages[i].blocksRaycasts = isFirst;
        }
        
        if (dialogueToast != null) { dialogueToast.alpha = 0f; dialogueToast.gameObject.SetActive(false); }
        if (fragmentOverlay != null) { fragmentOverlay.alpha = 0f; fragmentOverlay.gameObject.SetActive(false); }

        // --- 3. 事件绑定 ---
        btnPrev.onClick.AddListener(OnPrevClicked);
        btnNext.onClick.AddListener(OnNextClicked);
        
        // 关键：绑定合上图鉴按钮
        if(btnCloseGame != null) 
            btnCloseGame.onClick.AddListener(TriggerCloseUI);
        else
            Debug.LogWarning("【未赋值】Btn Close Game 尚未在 Inspector 中分配。");

        foreach (var option in pageOptions)
        {
            if (option.btnTellHim != null)
            {
                bool correct = option.isCorrect;
                string msg = option.feedback;
                option.btnTellHim.onClick.AddListener(() => OnTellHimClicked(correct, msg));
            }
        }

        UpdateNavUI();
    }

    private void OnPrevClicked() => SwitchPage(currentPage - 1);
    private void OnNextClicked() => SwitchPage(currentPage + 1);

    private void SwitchPage(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= pages.Length) return;

        HideDialogueToast();

        pages[currentPage].interactable = false;
        pages[currentPage].blocksRaycasts = false;
        pages[currentPage].DOFade(0f, 0.3f);

        currentPage = targetIndex;

        pages[currentPage].DOFade(1f, 0.3f).OnComplete(() => {
            pages[currentPage].interactable = true;
            pages[currentPage].blocksRaycasts = true;
        });

        UpdateNavUI();
    }

    private void UpdateNavUI()
    {
        if (btnPrev != null) btnPrev.interactable = (currentPage > 0);
        if (btnNext != null) btnNext.interactable = (currentPage < pages.Length - 1);
        if (pageIndicatorText != null) pageIndicatorText.text = $"{currentPage + 1} / {pages.Length}";
    }

    public void OnTellHimClicked(bool isCorrect, string feedback)
    {
        if (!isCorrect) ShowDialogueToast(feedback);
        else TriggerSuccessEnding();
    }

    private void ShowDialogueToast(string msg)
    {
        if (dialogueToast == null) return;
        dialogueToast.gameObject.SetActive(true);
        dialogueText.text = msg;
        currentDialogueTween?.Kill();

        Sequence seq = DOTween.Sequence();
        seq.Append(dialogueToast.DOFade(1f, 0.4f));
        seq.AppendInterval(3f);
        seq.Append(dialogueToast.DOFade(0f, 0.4f));
        seq.OnComplete(() => dialogueToast.gameObject.SetActive(false));
        currentDialogueTween = seq;
    }

    private void HideDialogueToast()
    {
        currentDialogueTween?.Kill();
        if (dialogueToast != null) { dialogueToast.alpha = 0f; dialogueToast.gameObject.SetActive(false); }
    }

    private void TriggerSuccessEnding()
    {
        HideDialogueToast();
        if (fragmentOverlay != null)
        {
            fragmentOverlay.gameObject.SetActive(true);
            fragmentOverlay.DOFade(1f, 1.2f).SetEase(Ease.OutCubic);
        }
    }

    // 真正的关闭逻辑
    private void TriggerCloseUI()
    {
        if (mainCanvasGroup != null)
        {
            // 整个 UI 淡出并彻底禁用
            mainCanvasGroup.DOFade(0f, 0.8f).OnComplete(() => {
                gameObject.SetActive(false);
                Debug.Log("图鉴已合上，返回主游戏。");
            });
        }
        else
        {
            // 如果没拖 mainCanvasGroup，就直接关掉
            gameObject.SetActive(false);
        }
    }
}