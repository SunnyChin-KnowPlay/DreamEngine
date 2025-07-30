using System.Threading.Tasks;

namespace MysticIsle.DreamEngine.States
{
    public interface IState
    {
        /// <summary>
        /// 当进入该状态时调用
        /// </summary>
        Task OnEnter();

        /// <summary>
        /// 当退出该状态时调用
        /// </summary>
        Task OnExit();

        /// <summary>
        /// 在状态中每帧更新时调用
        /// </summary>
        Task OnUpdate();
    }

}
