namespace BS.Gameplay.Puzzles
{
    /// <summary>
    /// 谜题状态。
    /// Inactive: 尚未激活或条件未满足
    /// Available: 可进行解题
    /// Solved: 已解开
    /// </summary>
    public enum PuzzleState
    {
        Inactive = 0,
        Available = 1,
        Solved = 2
    }
}
