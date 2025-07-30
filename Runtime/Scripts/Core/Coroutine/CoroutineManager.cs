using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MysticIsle.DreamEngine
{
    /// <summary>
    /// 协程管理器
    /// </summary>
    public sealed class CoroutineManager : Singleton<CoroutineManager>
    {
        private readonly Queue<IEnumerator> coroutineQueue = new();
        private Coroutine currentCoroutine = null;

        /// <summary>
        /// 添加协程到队列
        /// </summary>
        /// <param name="coroutine">要执行的协程</param>
        /// <param name="executeImmediately">是否立刻执行，如果false则下一帧执行(仅当此协程为队列中首位)</param>
        public void EnqueueCoroutine(IEnumerator coroutine, bool executeImmediately = true)
        {
            coroutineQueue.Enqueue(coroutine);

            if (executeImmediately && currentCoroutine == null)
            {
                ProcessNextCoroutine();
            }
        }

        private void Update()
        {
            if (currentCoroutine == null && coroutineQueue.Count > 0)
            {
                ProcessNextCoroutine();
            }
        }

        private void ProcessNextCoroutine()
        {
            if (coroutineQueue.Count > 0)
            {
                var coroutine = coroutineQueue.Dequeue();

                // 检查协程是否绑定到一个已经销毁的 GameObject
                if (IsCoroutineValid(coroutine))
                {
                    currentCoroutine = StartCoroutine(RunCoroutine(coroutine));
                }
                else
                {
                    // 如果协程无效，立即处理下一个
                    ProcessNextCoroutine();
                }
            }
        }

        private IEnumerator RunCoroutine(IEnumerator coroutine)
        {
            yield return coroutine;

            currentCoroutine = null;
            ProcessNextCoroutine(); // 当前协程执行完毕，立即检查并执行下一个
        }

        /// <summary>
        /// 清空协程队列并停止当前协程
        /// </summary>
        public new void StopAllCoroutines()
        {
            base.StopAllCoroutines();
            coroutineQueue.Clear();
            currentCoroutine = null;
        }

        /// <summary>
        /// 检查当前是否有正在执行的协程
        /// </summary>
        public bool IsProcessing()
        {
            return currentCoroutine != null;
        }

        /// <summary>
        /// 停止当前执行的协程
        /// </summary>
        public void StopCoroutine()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }
        }

        /// <summary>
        /// 检查协程是否绑定到一个有效的 GameObject
        /// </summary>
        private bool IsCoroutineValid(IEnumerator coroutine)
        {
            // 假设协程的第一个 yield 语句是等待一个 MonoBehaviour
            if (coroutine.Current is YieldInstruction yieldInstruction)
            {
                if (yieldInstruction is WaitForEndOfFrame || yieldInstruction is WaitForSeconds || yieldInstruction is WaitForFixedUpdate)
                {
                    // 这些类型的 YieldInstruction 不涉及 GameObject 的存在性
                    return true;
                }

                var monoBehaviour = coroutine as MonoBehaviour;
                if (monoBehaviour != null && monoBehaviour.gameObject != null)
                {
                    return monoBehaviour.gameObject != null;
                }
            }

            // 如果无法确定，默认认为是有效的
            return true;
        }
    }
}
