namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable, EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class InternalErrorException : Exception
    {
        public InternalErrorException() : base(Utils.GetResourceString("InternalError"))
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InternalErrorException(string message) : base(message)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private InternalErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InternalErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

