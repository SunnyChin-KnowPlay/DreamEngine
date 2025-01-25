namespace MysticIsle.DreamEngine.States
{
    public interface IState
    {
        /// <summary>
        /// 当进入该状态时调用
        /// </summary>
        void Enter();

        /// <summary>
        /// 当退出该状态时调用
        /// </summary>
        void Exit();

        /// <summary>
        /// 在状态中每帧更新时调用
        /// </summary>
        void Update();
    }

}
