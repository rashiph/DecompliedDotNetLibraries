namespace Microsoft.VisualBasic.ApplicationServices
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable, EditorBrowsable(EditorBrowsableState.Never)]
    public class NoStartupFormException : Exception
    {
        public NoStartupFormException() : base(Utils.GetResourceString("AppModel_NoStartupForm"))
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public NoStartupFormException(string message) : base(message)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected NoStartupFormException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public NoStartupFormException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

