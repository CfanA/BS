using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class DiaryPage
{
    [Header("页面配置")]
    public string pageName = "新页面";
    public CanvasGroup pageUI;       // 该页的日记文本与槽位节点
    public CanvasGroup fragmentPool; // 该页对应的底部碎片池节点
    
    [HideInInspector] public int totalBlanks;
    [HideInInspector] public int filledCount;
}

public class DiaryPuzzleManager : MonoBehaviour
{
    public static DiaryPuzzleManager Instance;

    [Header("全局引用")]
    public CanvasGroup mainCanvasGroup;
    public TMP_Text systemMessageText;
    public RectTransform diaryPanel;
    
    [Header("多页日记配置 (按顺序拖入)")]
    public List<DiaryPage> diaryPages = new List<DiaryPage>();
    
    [Header("结局演出引用")]
    public CanvasGroup blackoutOverlay;
    public TMP_Text endingText;

    private int currentPageIndex = 0;
    private Tween msgTween;
    private bool isTransitioning = false; // 防止翻页动画期间拖拽

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 结局黑屏初始化
        if(blackoutOverlay != null)
        {
            blackoutOverlay.alpha = 0f;
            blackoutOverlay.gameObject.SetActive(false);
            endingText.alpha = 0f;
        }

        // 初始化多页状态
        for (int i = 0; i < diaryPages.Count; i++)
        {
            DiaryPage page = diaryPages[i];
            // 自动统计这一页有几个 DropZone 需要填
            page.totalBlanks = page.pageUI.GetComponentsInChildren<DropZone>().Length;
            page.filledCount = 0;

            bool isFirstPage = (i == 0);
            
            // 只有第一页是可见且可交互的
            page.pageUI.alpha = isFirstPage ? 1f : 0f;
            page.pageUI.blocksRaycasts = isFirstPage;
            
            if (page.fragmentPool != null)
            {
                page.fragmentPool.alpha = isFirstPage ? 1f : 0f;
                page.fragmentPool.blocksRaycasts = isFirstPage;
            }
        }
    }

    public void ProcessDrop(DraggableFragment fragment, DropZone zone)
    {
        if (isTransitioning) 
        {
            fragment.SnapBack();
            return;
        }

        DiaryPage currentPage = diaryPages[currentPageIndex];

        // 1. 试图填入“真相” -> 报错震动
        if (fragment.isTruth)
        {
            ShowSystemMessage("不行……不能写这个。这不是属于今天的回忆。", new Color(0.8f, 0.2f, 0.2f));
            ShakeDiary();
            fragment.SnapBack();
            return;
        }

        // 2. 填入“谎言”但位置不对
        if (fragment.fragmentID != zone.targetFragmentID)
        {
            ShowSystemMessage("好像放错位置了……", Color.gray);
            fragment.SnapBack();
            return;
        }

        // 3. 正确填入
        fragment.transform.SetParent(zone.transform);
        fragment.transform.localPosition = Vector3.zero;
        
        if (fragment.TryGetComponent(out Image fragImg)) fragImg.enabled = false;
        fragment.GetComponent<CanvasGroup>().blocksRaycasts = false;
        zone.isFilled = true;
        
        currentPage.filledCount++;
        systemMessageText.DOFade(0f, 0.3f);

        // 检查当前页是否填满
        if (currentPage.filledCount >= currentPage.totalBlanks)
        {
            TurnToNextPage();
        }
    }

    private void TurnToNextPage()
    {
        isTransitioning = true;
        DiaryPage oldPage = diaryPages[currentPageIndex];
        currentPageIndex++;

        // 如果是最后一页，触发大结局
        if (currentPageIndex >= diaryPages.Count)
        {
            TriggerEndingSequence(oldPage);
            return;
        }

        DiaryPage newPage = diaryPages[currentPageIndex];
        Sequence turnSeq = DOTween.Sequence();

        // 停顿 0.5 秒让玩家看完拼好的一句话
        turnSeq.AppendInterval(0.5f);

        // 老页面淡出
        turnSeq.Append(oldPage.pageUI.DOFade(0f, 0.5f));
        if (oldPage.fragmentPool != null) turnSeq.Join(oldPage.fragmentPool.DOFade(0f, 0.5f));
        
        turnSeq.AppendCallback(() => {
            oldPage.pageUI.blocksRaycasts = false;
            if (oldPage.fragmentPool != null) oldPage.fragmentPool.blocksRaycasts = false;
            
            // 这里可以播放翻页音效：AudioManager.Play("PageTurn");
        });

        // 新页面淡入
        turnSeq.Append(newPage.pageUI.DOFade(1f, 0.5f));
        if (newPage.fragmentPool != null) turnSeq.Join(newPage.fragmentPool.DOFade(1f, 0.5f));
        
        turnSeq.OnComplete(() => {
            newPage.pageUI.blocksRaycasts = true;
            if (newPage.fragmentPool != null) newPage.fragmentPool.blocksRaycasts = true;
            isTransitioning = false;
            
            ShowSystemMessage($"[ 翻到了新的一页 ]", Color.gray);
        });
    }

    private void ShowSystemMessage(string msg, Color color)
    {
        msgTween?.Kill();
        systemMessageText.text = msg;
        systemMessageText.color = color;
        
        Sequence seq = DOTween.Sequence();
        seq.Append(systemMessageText.DOFade(1f, 0.2f));
        seq.AppendInterval(2.5f);
        seq.Append(systemMessageText.DOFade(0f, 0.5f));
        seq.OnComplete(() => {
            systemMessageText.text = "[拖拽记忆碎片，补全今天的日记]";
            systemMessageText.color = Color.gray;
            systemMessageText.DOFade(1f, 0.5f);
        });
        msgTween = seq;
    }

    private void ShakeDiary()
    {
        diaryPanel.DOComplete();
        diaryPanel.DOShakePosition(0.4f, new Vector3(15f, 0, 0), 20, 90, false, true);
    }

    private void TriggerEndingSequence(DiaryPage lastPage)
    {
        if (lastPage.fragmentPool != null) 
            lastPage.fragmentPool.DOFade(0f, 0.5f).OnComplete(() => lastPage.fragmentPool.blocksRaycasts = false);

        Sequence endSeq = DOTween.Sequence();
        endSeq.AppendInterval(1.0f);
        endSeq.AppendCallback(() => {
            diaryPanel.DOPunchScale(new Vector3(0.05f, 0.05f, 0), 0.3f, 5, 0.5f);
            blackoutOverlay.gameObject.SetActive(true);
            blackoutOverlay.DOFade(1f, 0.1f);
        });
        
        endSeq.AppendInterval(1.5f);
        endSeq.Append(endingText.DOFade(1f, 1.5f));
        endSeq.AppendInterval(3.0f);
        
        endSeq.AppendCallback(() => {
            if (mainCanvasGroup != null)
            {
                foreach (Transform child in mainCanvasGroup.transform)
                    if (child != blackoutOverlay.transform) child.gameObject.SetActive(false);
            }
        });

        if (mainCanvasGroup != null)
        {
            endSeq.Append(mainCanvasGroup.DOFade(0f, 1.5f));
            endSeq.OnComplete(() => {
                mainCanvasGroup.gameObject.SetActive(false);
                Debug.Log("日记写完，第一章结束。");
            });
        }
        else
        {
            endSeq.OnComplete(() => gameObject.SetActive(false));
        }
    }
}