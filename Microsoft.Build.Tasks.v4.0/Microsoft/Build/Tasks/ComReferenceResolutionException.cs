namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ComReferenceResolutionException : Exception
    {
        internal ComReferenceResolutionException()
        {
        }

        internal ComReferenceResolutionException(Exception innerException) : base("", innerException)
        {
        }

        protected ComReferenceResolutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

