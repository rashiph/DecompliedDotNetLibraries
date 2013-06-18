namespace Microsoft.VisualBasic.ApplicationServices
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable, EditorBrowsable(EditorBrowsableState.Never)]
    public class CantStartSingleInstanceException : Exception
    {
        public CantStartSingleInstanceException() : base(Utils.GetResourceString("AppModel_SingleInstanceCantConnect"))
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CantStartSingleInstanceException(string message) : base(message)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CantStartSingleInstanceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CantStartSingleInstanceException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

