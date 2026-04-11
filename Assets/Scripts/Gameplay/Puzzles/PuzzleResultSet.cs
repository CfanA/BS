using System;
using BS.Core;
using BS.Gameplay.Story.Actions;

namespace BS.Gameplay.Puzzles
{
    /// <summary>
    /// 谜题解开后的结果集合。
    /// 复用现有剧情动作系统，避免谜题系统重复发明一套结果执行器。
    /// </summary>
    [Serializable]
    public sealed class PuzzleResultSet
    {
        [UnityEngine.SerializeField] private StoryAction[] actions;

        public void Execute(GameManager gameManager)
        {
            if (gameManager == null || actions == null)
            {
                return;
            }

            for (var i = 0; i < actions.Length; i++)
            {
                actions[i]?.Execute(gameManager);
            }
        }
    }
}
