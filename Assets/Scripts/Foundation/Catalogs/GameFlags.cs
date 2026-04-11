using BS.Foundation.Ids;

namespace BS.Foundation.Catalogs
{
    /// <summary>
    /// 集中定义项目内常用剧情标记。
    /// 后续新增标记时，优先从这里扩展，避免业务代码手写字符串。
    /// </summary>
    public static class GameFlags
    {
        public static readonly FlagId IntroFinished = new("story.intro_finished");
        public static readonly FlagId FirstDoorUnlocked = new("puzzle.first_door_unlocked");
        public static readonly FlagId FoundOldKey = new("clue.found_old_key");
        public static readonly FlagId MetMysteriousGirl = new("dialog.met_mysterious_girl");
        public static readonly FlagId InvestigatedDesk = new("story.investigated_desk");
        public static readonly FlagId ExaminedLockedDoor = new("story.examined_locked_door");
    }
}
