using BS.Gameplay.Dialogue.Data;

namespace BS.Gameplay.Dialogue
{
    /// <summary>
    /// 当前对话会话状态。
    /// 把运行时索引和数据引用从管理器主类里抽出来，便于后续扩展分支选择。
    /// </summary>
    public sealed class DialogueSession
    {
        public DialogueSession(DialogueData dialogueData)
        {
            DialogueData = dialogueData;
            CurrentLineIndex = -1;
        }

        public DialogueData DialogueData { get; }
        public int CurrentLineIndex { get; private set; }
        public bool HasNextLine => CurrentLineIndex + 1 < DialogueData.Lines.Count;
        public DialogueLine CurrentLine =>
            CurrentLineIndex >= 0 && CurrentLineIndex < DialogueData.Lines.Count
                ? DialogueData.Lines[CurrentLineIndex]
                : null;

        public DialogueLine MoveNext()
        {
            if (!HasNextLine)
            {
                return null;
            }

            CurrentLineIndex++;
            return DialogueData.Lines[CurrentLineIndex];
        }
    }
}
