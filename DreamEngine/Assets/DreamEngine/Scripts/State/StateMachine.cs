using System.Collections.Generic;
using UnityEngine;

namespace MysticIsle.DreamEngine.States
{
    public class StateMachine : MonoBehaviour
    {
        private State currentState;
        private readonly Dictionary<string, State> states = new();

        private Coroutine currentCoroutine;

        /// <summary>
        /// 添加状态到状态机
        /// </summary>
        public void AddState(string name, State state)
        {
            if (!states.ContainsKey(name))
            {
                states[name] = state;
            }
        }

        /// <summary>
        /// 切换到指定状态，支持协程
        /// </summary>
        public void SwitchState(string name)
        {
            if (states.ContainsKey(name))
            {
                currentState?.Exit(); // 退出当前状态
                currentState = states[name];

                // 如果有当前协程在运行，停止它
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }

                // 进入新状态并运行协程
                currentCoroutine = StartCoroutine(currentState.Enter());
            }
            else
            {
                Debug.LogError($"State {name} not found in the state machine.");
            }
        }

        /// <summary>
        /// 更新当前状态，支持协程
        /// </summary>
        public void UpdateState()
        {
            if (currentState != null && currentCoroutine == null)
            {
                currentCoroutine = StartCoroutine(currentState.Update());
            }
        }
    }
}
