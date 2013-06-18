namespace System.ServiceProcess
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class TimeoutException : SystemException
    {
        public TimeoutException()
        {
            base.HResult = -2146232058;
        }

        public TimeoutException(string message) : base(message)
        {
            base.HResult = -2146232058;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TimeoutException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232058;
        }
    }
}

