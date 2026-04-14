using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using DG.Tweening;

public class GeminiWhisperManager : MonoBehaviour
{
    [Header("API 配置")]
    private string apiKey = "YOUR_GEMINI_API_KEY_HERE"; // 替换为你的真实 Key
    private string apiUrl = "[https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent?key=](https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent?key=)";

    [Header("UI 引用")]
    public CanvasGroup modalGroup;
    public CanvasGroup inputGroup;
    public CanvasGroup responseGroup;
    public TMP_InputField inputField;
    public TMP_Text responseText;

    private const string SYSTEM_PROMPT = "你是一个2D独立叙事游戏里的环境旁白。游戏氛围是克制、压抑、低饱和的夏末。玩家正在对一个【已经在事故中离去的挚友】说话，试图维持他还活着的幻象。请用极简、充满物哀感、隐喻的语言回复（限制在30个汉字以内）。绝对不要直接以人的身份回答，不要说“我听到了”。你必须描写周遭环境极其微小的变化（比如：风吹动了野草、桌上的水杯落了灰、走廊的嗡嗡声更大了等），来隐晦地暗示那个人已经不在了，只有无尽的虚空在回应。";

    void Start()
    {
        modalGroup.alpha = 0;
        modalGroup.blocksRaycasts = false;
        
        // 监听回车键
        inputField.onSubmit.AddListener(delegate { SendWhisper(); });
    }

    public void OpenWhisper()
    {
        inputField.text = "";
        inputGroup.alpha = 1;
        inputGroup.blocksRaycasts = true;
        responseGroup.alpha = 0;
        responseGroup.blocksRaycasts = false;

        modalGroup.blocksRaycasts = true;
        modalGroup.DOFade(1f, 1.2f).SetEase(Ease.OutCubic).OnComplete(() => {
            inputField.ActivateInputField(); // 自动聚焦
        });
    }

    public void CloseWhisper()
    {
        modalGroup.DOFade(0f, 1f).SetEase(Ease.InOutCubic).OnComplete(() => {
            modalGroup.blocksRaycasts = false;
        });
    }

    public void SendWhisper()
    {
        string userText = inputField.text.Trim();
        if (string.IsNullOrEmpty(userText)) return;

        // 隐藏输入框，显示等待提示
        inputGroup.blocksRaycasts = false;
        inputGroup.DOFade(0f, 0.8f);
        
        responseText.text = "风正在带走你的声音...";
        responseText.color = Color.gray;
        responseGroup.DOFade(1f, 1f);

        StartCoroutine(PostToGemini(userText));
    }

    private IEnumerator PostToGemini(string userText)
    {
        // 1. 构建严格的 Gemini Payload 结构
        GeminiRequest payload = new GeminiRequest
        {
            contents = new List<Content> { 
                new Content { parts = new List<Part> { new Part { text = $"玩家在虚空中说出了这句话：\"{userText}\"" } } } 
            },
            systemInstruction = new SystemInstruction {
                parts = new List<Part> { new Part { text = SYSTEM_PROMPT } }
            }
        };

        string jsonPayload = JsonUtility.ToJson(payload);

        // 2. 发送请求
        using (UnityWebRequest request = new UnityWebRequest(apiUrl + apiKey, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            // 3. 处理响应动画 (先淡出"等待中", 再淡入"AI回复")
            Sequence seq = DOTween.Sequence();
            seq.Append(responseGroup.DOFade(0f, 0.8f));
            
            seq.AppendCallback(() => {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        GeminiResponse res = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
                        string reply = res.candidates[0].content.parts[0].text;
                        responseText.text = $"「 {reply.Trim()} 」";
                        responseText.color = new Color(0.9f, 0.9f, 0.9f); // 变白一点
                    }
                    catch { responseText.text = "（风声太大，你的声音散落在了半空...）"; }
                }
                else
                {
                    responseText.text = "（只有远处的门轴发出一声沉闷的叹息...）";
                }
            });

            seq.Append(responseGroup.DOFade(1f, 1.2f).SetEase(Ease.InOutSine));
            seq.OnComplete(() => responseGroup.blocksRaycasts = true);
        }
    }

    #region Gemini JSON 序列化类 (配合 Unity JsonUtility)
    [Serializable] public class GeminiRequest { public List<Content> contents; public SystemInstruction systemInstruction; }
    [Serializable] public class Content { public List<Part> parts; }
    [Serializable] public class Part { public string text; }
    [Serializable] public class SystemInstruction { public List<Part> parts; }
    [Serializable] public class GeminiResponse { public List<Candidate> candidates; }
    [Serializable] public class Candidate { public Content content; }
    #endregion
}