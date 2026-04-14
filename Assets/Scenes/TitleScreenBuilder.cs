using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleScreenBuilder : MonoBehaviour
{
    [Header("全局资源引用 (请在 Inspector 拖入)")]
    public TMP_FontAsset mainFont;
    public Sprite bgSprite;
    public Sprite vignetteSprite;

    [ContextMenu("一键生成主界面 UI")]
    public void BuildTitleScreen()
    {
        // 1. 创建 Canvas
        GameObject canvasObj = new GameObject("Canvas_TitleScreen");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. 背景层 (Bg_Atmosphere)
        GameObject bgObj = CreateUIElement("Bg_Atmosphere", canvasObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.sprite = bgSprite;
        bgImage.color = new Color(0.1f, 0.1f, 0.12f, 1f); // 偏暗的冷色调
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = new Vector2(150f, 150f); // 显著放大以便支持更强的视差移动
        MouseParallax bgParallax = bgObj.AddComponent<MouseParallax>();
        bgParallax.moveAmount = 40f; // 加强背景视差幅度

        // 3. 粒子系统层 (Particle_Dust)
        GameObject particleObj = new GameObject("Particle_Dust");
        particleObj.transform.SetParent(canvasObj.transform, false);
        particleObj.AddComponent<ParticleSystem>(); // 具体增强参数仍建议在 Inspector 参照文档调整

        // 4. 暗角遮罩 (Vignette_Overlay)
        GameObject vignetteObj = CreateUIElement("Vignette_Overlay", canvasObj.transform);
        Image vignetteImage = vignetteObj.AddComponent<Image>();
        vignetteImage.sprite = vignetteSprite;
        vignetteImage.raycastTarget = false; // 务必取消点击拦截
        vignetteImage.color = new Color(0, 0, 0, 0.85f);
        SetFullScreen(vignetteObj.GetComponent<RectTransform>());

        // 5. UI 容器 (UI_Container)
        GameObject uiContainer = CreateUIElement("UI_Container", canvasObj.transform);
        RectTransform uiRect = uiContainer.GetComponent<RectTransform>();
        SetFullScreen(uiRect);
        uiRect.offsetMin = new Vector2(200f, 0); // 靠左布局
        MouseParallax uiParallax = uiContainer.AddComponent<MouseParallax>();
        uiParallax.moveAmount = -20f; // 加强 UI 反向视差幅度

        // 6. 标题组 (Title_Group)
        GameObject titleGroup = CreateUIElement("Title_Group", uiContainer.transform);
        VerticalLayoutGroup titleLayout = titleGroup.AddComponent<VerticalLayoutGroup>();
        titleLayout.childAlignment = TextAnchor.MiddleLeft;
        titleLayout.spacing = 10f;
        titleGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, 150);

        CreateText("Text_Chapter", titleGroup.transform, "第一章：靠得太近的时候", 24, Color.gray);
        TMP_Text mainTitle = CreateText("Text_MainTitle", titleGroup.transform, "夏 日 漫 长", 80, Color.white);
        mainTitle.gameObject.AddComponent<TitleBreatheAnimator>();

        // 6.5 底部系统提示框 (用于显示"还未开发")
        TMP_Text systemMsgText = CreateText("Text_SystemMsg", uiContainer.transform, "", 20, new Color(0.6f, 0.6f, 0.6f, 0f));
        systemMsgText.alignment = TextAlignmentOptions.Left;
        systemMsgText.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, -280); // 放在菜单下方

        // 7. 菜单组 (Menu_Group)
        GameObject menuGroup = CreateUIElement("Menu_Group", uiContainer.transform);
        VerticalLayoutGroup menuLayout = menuGroup.AddComponent<VerticalLayoutGroup>();
        menuLayout.childAlignment = TextAnchor.MiddleLeft;
        menuLayout.spacing = 20f;
        menuGroup.GetComponent<RectTransform>().anchoredPosition = new Vector2(50, -50);

        // 创建按钮并赋予对应的逻辑类型
        CreateMenuButton("Btn_Start", menuGroup.transform, "开 始", 1.5f, MenuButtonAnimator.ActionType.Start, systemMsgText);
        CreateMenuButton("Btn_Load", menuGroup.transform, "载 入", 1.8f, MenuButtonAnimator.ActionType.ShowMessage, systemMsgText);
        CreateMenuButton("Btn_Options", menuGroup.transform, "选 项", 2.1f, MenuButtonAnimator.ActionType.ShowMessage, systemMsgText);
        CreateMenuButton("Btn_Quit", menuGroup.transform, "退 出", 2.4f, MenuButtonAnimator.ActionType.Quit, systemMsgText);

        Debug.Log("主界面 UI 及组件硬编码生成完成！");
    }

    // 辅助构建方法
    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private TMP_Text CreateText(string name, Transform parent, string content, float fontSize, Color color)
    {
        GameObject go = CreateUIElement(name, parent);
        TMP_Text txt = go.AddComponent<TextMeshProUGUI>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = TextAlignmentOptions.Center;
        if (mainFont != null) txt.font = mainFont;
        return txt;
    }

    private Button CreateMenuButton(string name, Transform parent, string text, float delay, MenuButtonAnimator.ActionType actionType, TMP_Text msgBox)
    {
        GameObject btnObj = CreateUIElement(name, parent);
        Button btn = btnObj.AddComponent<Button>();
        
        TMP_Text btnText = CreateText("Text", btnObj.transform, text, 36, Color.gray);
        btnText.alignment = TextAlignmentOptions.Left; // 菜单按钮居左
        
        MenuButtonAnimator anim = btnObj.AddComponent<MenuButtonAnimator>();
        anim.buttonText = btnText;
        anim.introDelay = delay;
        anim.actionType = actionType;
        anim.systemMsgBox = msgBox; // 绑定提示框

        return btn;
    }

    private void SetFullScreen(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}