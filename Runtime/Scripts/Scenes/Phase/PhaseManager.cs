using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace MysticIsle.DreamEngine.Phases
{
    /// <summary>
    /// Manages the phases of the game, allowing for phase transitions and interruptions.
    /// </summary>
    public class PhaseManager : MonoBehaviour, IManager
    {
        #region Fields

        /// <summary>
        /// Queue to manage the phases.
        /// </summary>
        private readonly Queue<IPhase> phaseQueue = new();

        /// <summary>
        /// The current phase being executed.
        /// </summary>
        private IPhase currentPhase;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current phase.
        /// </summary>
        public IPhase CurrentPhase => currentPhase;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new phase to the queue.
        /// </summary>
        /// <param name="phase">The phase to enqueue.</param>
        public void EnqueuePhase(IPhase phase)
        {
            phaseQueue.Enqueue(phase);
        }

        /// <summary>
        /// Updates the current phase each frame.
        /// </summary>
        public virtual void Update()
        {
            if (currentPhase == null && phaseQueue.Count > 0)
            {
                // If there is no current phase and the queue has phases
                currentPhase = phaseQueue.Dequeue(); // Get the next phase

                // If the current phase exists, execute the update
                if (currentPhase != null)
                {
                    RunCurrentPhase().Forget();
                }
            }
        }

        /// <summary>
        /// Executes the current phase.
        /// </summary>
        private async UniTask RunCurrentPhase()
        {
            // Execute the phase's run logic
            await currentPhase.Run();
            currentPhase = null; // Phase execution complete, clear the current phase
        }

        /// <summary>
        /// Allows external interruption of the current phase.
        /// </summary>
        public void InterruptCurrentPhase()
        {
            currentPhase?.Interrupt();
        }

        /// <summary>
        /// Switches to a new phase and adds it to the queue.
        /// </summary>
        /// <param name="newPhase">The new phase to switch to.</param>
        public void SwitchPhase(IPhase newPhase)
        {
            InterruptCurrentPhase();
            EnqueuePhase(newPhase);  // Add the new phase to the queue
        }

        #endregion
    }
}
