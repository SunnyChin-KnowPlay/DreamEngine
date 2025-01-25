using System;
using System.Collections;

namespace MysticIsle.DreamEngine.States
{
    public class State
    {
        private readonly IState stateImplementation;
        private readonly Func<IEnumerator> onEnter;
        private readonly Action onExit;
        private readonly Func<IEnumerator> onUpdate;

        public State(IState state)
        {
            stateImplementation = state;
        }

        public State(Func<IEnumerator> enter, Action exit, Func<IEnumerator> update)
        {
            onEnter = enter;
            onExit = exit;
            onUpdate = update;
        }

        public IEnumerator Enter()
        {
            if (stateImplementation != null)
            {
                stateImplementation.Enter();
                yield break;
            }

            if (onEnter != null)
            {
                yield return onEnter();
            }
        }

        public void Exit()
        {
            if (stateImplementation != null)
            {
                stateImplementation.Exit();
                return;
            }

            onExit?.Invoke();
        }

        public IEnumerator Update()
        {
            if (stateImplementation != null)
            {
                stateImplementation.Update();
                yield break;
            }

            if (onUpdate != null)
            {
                yield return onUpdate();
            }
        }
    }
}
