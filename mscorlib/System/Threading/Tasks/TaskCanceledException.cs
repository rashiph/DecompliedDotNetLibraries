namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TaskCanceledException : OperationCanceledException
    {
        [NonSerialized]
        private System.Threading.Tasks.Task m_canceledTask;

        public TaskCanceledException() : base(Environment.GetResourceString("TaskCanceledException_ctor_DefaultMessage"))
        {
        }

        public TaskCanceledException(string message) : base(message)
        {
        }

        public TaskCanceledException(System.Threading.Tasks.Task task) : base(Environment.GetResourceString("TaskCanceledException_ctor_DefaultMessage"), (task != null) ? task.CancellationToken : new CancellationToken())
        {
            this.m_canceledTask = task;
        }

        protected TaskCanceledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TaskCanceledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public System.Threading.Tasks.Task Task
        {
            get
            {
                return this.m_canceledTask;
            }
        }
    }
}

