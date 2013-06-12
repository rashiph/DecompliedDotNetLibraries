namespace System
{
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class InsufficientExecutionStackException : SystemException
    {
        public InsufficientExecutionStackException() : base(Environment.GetResourceString("Arg_InsufficientExecutionStackException"))
        {
            base.SetErrorCode(-2146232968);
        }

        public InsufficientExecutionStackException(string message) : base(message)
        {
            base.SetErrorCode(-2146232968);
        }

        private InsufficientExecutionStackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InsufficientExecutionStackException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146232968);
        }
    }
}

