using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MysticIsle.DreamEngine.States
{
    public class StateMachine : MonoBehaviour
    {
        private State currentState;
        private readonly Dictionary<string, State> states = new();

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
        /// 切换到指定状态
        /// </summary>
        public async Task SwitchState(string name)
        {
            if (states.ContainsKey(name))
            {
                if (currentState != null)
                {
                    await currentState.Exit(); // 退出当前状态
                }
                currentState = states[name];
                await currentState.Enter(); // 进入新状态
            }
            else
            {
                Debug.LogError($"State {name} not found in the state machine.");
            }
        }

        /// <summary>
        /// 更新当前状态
        /// </summary>
        public async Task UpdateState()
        {
            if (currentState != null)
            {
                await currentState.Update();
            }
        }
    }
}
