namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class StrongNameException : Exception
    {
        internal StrongNameException()
        {
        }

        internal StrongNameException(Exception innerException) : base("", innerException)
        {
        }

        protected StrongNameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

