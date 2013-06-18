namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class ReferenceResolutionException : Exception
    {
        private ReferenceResolutionException()
        {
        }

        private ReferenceResolutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal ReferenceResolutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

