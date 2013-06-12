namespace System.Threading.Tasks
{
    using System;

    internal class SystemThreadingTasks_FutureDebugView<TResult>
    {
        private Task<TResult> m_task;

        public SystemThreadingTasks_FutureDebugView(Task<TResult> task)
        {
            this.m_task = task;
        }

        public object AsyncState
        {
            get
            {
                return this.m_task.AsyncState;
            }
        }

        public bool CancellationPending
        {
            get
            {
                return ((this.m_task.Status == TaskStatus.WaitingToRun) && this.m_task.CancellationToken.IsCancellationRequested);
            }
        }

        public TaskCreationOptions CreationOptions
        {
            get
            {
                return this.m_task.CreationOptions;
            }
        }

        public System.Exception Exception
        {
            get
            {
                return this.m_task.Exception;
            }
        }

        public int Id
        {
            get
            {
                return this.m_task.Id;
            }
        }

        public TResult Result
        {
            get
            {
                if (this.m_task.Status != TaskStatus.RanToCompletion)
                {
                    return default(TResult);
                }
                return this.m_task.Result;
            }
        }

        public TaskStatus Status
        {
            get
            {
                return this.m_task.Status;
            }
        }
    }
}

