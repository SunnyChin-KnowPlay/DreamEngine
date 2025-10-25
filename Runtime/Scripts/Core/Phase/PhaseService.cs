using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// 阶段服务，负责按队列顺序执行游戏阶段并提供中断与切换功能。
    /// </summary>
    public class PhaseService : IGameService
    {
        #region Fields

        /// <summary>
        /// 待执行阶段的队列。
        /// </summary>
        [ShowInInspector]
        private readonly Queue<IPhase> phaseQueue = new();

        /// <summary>
        /// 当前正在运行的阶段。
        /// </summary>
        [ShowInInspector]
        private IPhase currentPhase;

        #endregion

        #region Properties

        /// <summary>
        /// 获取当前正在运行的阶段。
        /// </summary>
        public IPhase CurrentPhase => currentPhase;

        #endregion

        #region Methods

        /// <summary>
        /// 加入一个新的阶段等待执行。
        /// </summary>
        /// <param name="phase">阶段实例。</param>
        public void EnqueuePhase(IPhase phase)
        {
            phaseQueue.Enqueue(phase);
        }

        /// <summary>
        /// 服务的每帧更新，负责取出并启动下一个阶段。
        /// </summary>
        public virtual void OnUpdate()
        {
            if (currentPhase == null && phaseQueue.Count > 0)
            {
                currentPhase = phaseQueue.Dequeue();

                if (currentPhase != null)
                {
                    RunCurrentPhase().Forget();
                }
            }
        }

        /// <summary>
        /// 异步执行当前阶段并在完成后释放引用。
        /// </summary>
        private async UniTask RunCurrentPhase()
        {
            await currentPhase.Run();
            currentPhase = null;
        }

        /// <summary>
        /// 中断当前阶段的执行。
        /// </summary>
        public void InterruptCurrentPhase()
        {
            currentPhase?.Interrupt();
        }

        /// <summary>
        /// 立即中断当前阶段并切换到新的阶段。
        /// </summary>
        /// <param name="newPhase">新的阶段实例。</param>
        public void SwitchPhase(IPhase newPhase)
        {
            InterruptCurrentPhase();
            EnqueuePhase(newPhase);
        }
        #endregion
    }
}
