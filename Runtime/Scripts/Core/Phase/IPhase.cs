using Cysharp.Threading.Tasks;

namespace MysticIsle.DreamEngine
{
    public interface IPhase
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 运行
        /// </summary>
        UniTask Run();

        /// <summary>
        /// 中断
        /// </summary>
        void Interrupt();
    }

}
