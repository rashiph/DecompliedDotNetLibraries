namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class BadImageReferenceException : Exception
    {
        private BadImageReferenceException()
        {
        }

        private BadImageReferenceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal BadImageReferenceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

