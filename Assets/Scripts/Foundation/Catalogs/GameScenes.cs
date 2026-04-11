using BS.Foundation.Ids;

namespace BS.Foundation.Catalogs
{
    /// <summary>
    /// 集中定义项目内使用的场景。
    /// MVP 先保留一个样例场景，后续再扩展标题场景、关卡场景等。
    /// </summary>
    public static class GameScenes
    {
        public static readonly SceneId Sample = new("SampleScene");
    }
}
