using System;
using System.Threading;
using UnityEngine;

namespace RoslynCSharp
{
    /// <summary>
    /// Base class for awaitable async operations.
    /// </summary>
    public abstract class AsyncOperation : CustomYieldInstruction
    {
        // Private
        private bool hasStarted = false;
        private bool threadExit = false;
        private bool isDone = false;

        // Protected
        /// <summary>
        /// Was the async operation successful.
        /// </summary>
        protected bool isSuccessful = false;

        // Properties
        /// <summary>
        /// Returns true if the async operation has finished.
        /// </summary>
        public bool IsDone
        {
            get { return isDone; }
        }

        /// <summary>
        /// Retruns true if the async operation was successful.
        /// </summary>
        public bool IsSuccessful
        {
            get { return isSuccessful; }
        }

        /// <summary>
        /// Override implementation of CustomYieldInstruction.
        /// </summary>
        public override bool keepWaiting
        {
            get
            {
                if(hasStarted == false)
                {
                    // Start thread
                    ThreadPool.QueueUserWorkItem((object state) =>
                    {
                        try
                        {
                            // Set the flag
                            hasStarted = true;

                            // Run async code
                            RunAsyncOperation();
                        }
                        catch(Exception e)
                        {
                            Debug.LogException(e);
                            isSuccessful = false;
                        }

                        // Set exit flag
                        threadExit = true;
                    });
                }

                // CHeck if the thread has exited
                if (threadExit == false)
                    return true;

                // Run cleanup
                try
                {
                    // Run sync code
                    RunSyncFinalize();
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                    isSuccessful = false;
                }

                // Task has finished - stop waiting
                isDone = true;
                return false;
            }
        }

        // Methods
        /// <summary>
        /// Main entry point for async code.
        /// </summary>
        protected abstract void RunAsyncOperation();

        /// <summary>
        /// Main entry point for sync finalize code.
        /// </summary>
        protected virtual void RunSyncFinalize() { }
    }
}
