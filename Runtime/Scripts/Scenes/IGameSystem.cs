using MysticIsle.DreamEngine.Core;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// 游戏系统接口基类（替代旧的 IManager）。
    /// 作为标识接口使用，便于不同系统统一管理与查找。
    /// </summary>
    public interface IGameSystem : IUpdate
    {

    }
}
