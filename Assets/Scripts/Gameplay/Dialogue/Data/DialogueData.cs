using System;
using System.Collections.Generic;
using UnityEngine;

namespace BS.Gameplay.Dialogue.Data
{
    /// <summary>
    /// 单句对白数据。
    /// 当前只支持线性对白，并预留每句事件键用于触发剧情回调。
    /// </summary>
    [Serializable]
    public sealed class DialogueLine
    {
        [SerializeField] private string speakerName = "角色";
        [SerializeField] [TextArea(2, 6)] private string content = "对白内容";
        [SerializeField] private string eventKey;

        public string SpeakerName => speakerName;
        public string Content => content;
        public string EventKey => eventKey;
    }

    /// <summary>
    /// 对话资源。
    /// 用于配置一段线性对白，不直接依赖 UI 和场景对象。
    /// </summary>
    [CreateAssetMenu(menuName = "BS/Dialogue/Dialogue Data", fileName = "DialogueData_")]
    public sealed class DialogueData : ScriptableObject
    {
        [Header("对话信息")]
        [SerializeField] private string dialogueId = "dialog.sample";
        [SerializeField] private List<DialogueLine> lines = new();

        public string DialogueId => dialogueId;
        public IReadOnlyList<DialogueLine> Lines => lines;
        public bool IsValid => !string.IsNullOrWhiteSpace(dialogueId) && lines.Count > 0;
    }
}
