namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class DependencyResolutionException : Exception
    {
        private DependencyResolutionException()
        {
        }

        private DependencyResolutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal DependencyResolutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

