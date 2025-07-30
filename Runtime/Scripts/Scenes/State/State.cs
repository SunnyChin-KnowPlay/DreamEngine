using System;
using System.Threading.Tasks;

namespace MysticIsle.DreamEngine.States
{
    public class State
    {
        private readonly IState stateImplementation;
        private readonly Func<Task> onEnter;
        private readonly Func<Task> onExit;
        private readonly Func<Task> onUpdate;

        public State(IState state)
        {
            stateImplementation = state;
        }

        public State(Func<Task> enter, Func<Task> exit, Func<Task> update)
        {
            onEnter = enter;
            onExit = exit;
            onUpdate = update;
        }

        public async Task Enter()
        {
            if (stateImplementation != null)
            {
                await stateImplementation.OnEnter();
                return;
            }

            if (onEnter != null)
            {
                await onEnter();
            }
        }

        public async Task Exit()
        {
            if (stateImplementation != null)
            {
                await stateImplementation.OnExit();
                return;
            }

            if (onExit != null)
            {
                await onExit();
            }
        }

        public async Task Update()
        {
            if (stateImplementation != null)
            {
                await stateImplementation.OnUpdate();
                return;
            }

            if (onUpdate != null)
            {
                await onUpdate();
            }
        }
    }
}
