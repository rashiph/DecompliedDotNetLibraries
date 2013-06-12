namespace System.Threading.Tasks
{
    using System;

    internal class SystemThreadingTasks_TaskDebugView
    {
        private Task m_task;

        public SystemThreadingTasks_TaskDebugView(Task task)
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

        public TaskStatus Status
        {
            get
            {
                return this.m_task.Status;
            }
        }
    }
}

