namespace System.Threading.Tasks
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TaskSchedulerException : Exception
    {
        public TaskSchedulerException() : base(Environment.GetResourceString("TaskSchedulerException_ctor_DefaultMessage"))
        {
        }

        public TaskSchedulerException(Exception innerException) : base(Environment.GetResourceString("TaskSchedulerException_ctor_DefaultMessage"), innerException)
        {
        }

        public TaskSchedulerException(string message) : base(message)
        {
        }

        protected TaskSchedulerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TaskSchedulerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

