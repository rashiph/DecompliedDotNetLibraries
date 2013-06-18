namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable, EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class IncompleteInitialization : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IncompleteInitialization()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IncompleteInitialization(string message) : base(message)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private IncompleteInitialization(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IncompleteInitialization(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

