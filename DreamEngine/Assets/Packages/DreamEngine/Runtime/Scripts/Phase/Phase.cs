using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace MysticIsle.DreamEngine
{
    public class Phase : IPhase
    {
        #region Fields
        public virtual string Name => "默认阶段";

        /// <summary>
        /// CancellationToken for managing task cancellation
        /// </summary>
        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

        /// <summary>
        /// Source for generating cancellation tokens
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region Virtual Methods

        /// <summary>
        /// Method called when entering the phase
        /// </summary>
        protected virtual UniTask OnEnter()
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Method called when exiting the phase
        /// </summary>
        protected virtual UniTask OnExit()
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Method called for updating the phase asynchronously
        /// </summary>
        protected virtual UniTask OnUpdate()
        {
            return UniTask.CompletedTask;
        }

        #endregion

        #region Public Methods

        public Phase()
        {

        }

        /// <summary>
        /// Main method to run the phase
        /// </summary>
        public async UniTask Run()
        {
            // Create a new CancellationTokenSource for the entire phase
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Enter the phase
                await OnEnter();

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // Update the phase
                    await OnUpdate();
                }
            }
            catch (OperationCanceledException)
            {
                // Logic to handle task cancellation (can choose to ignore or log)
                UnityEngine.Debug.Log("Phase was interrupted and canceled.");
            }
            catch (Exception ex)
            {
                // Handle other exceptions (e.g., possible errors)
                UnityEngine.Debug.LogError($"Exception in phase: {ex}");
                throw;
            }
            finally
            {
                // Ensure OnExit is always called
                await OnExit();

                // Ensure _cancellationTokenSource is always disposed at the end of the phase
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Method to be called externally to interrupt the phase
        /// </summary>
        public void Interrupt()
        {
            _cancellationTokenSource?.Cancel(); // Issue a cancellation request
        }

        #endregion
    }
}
