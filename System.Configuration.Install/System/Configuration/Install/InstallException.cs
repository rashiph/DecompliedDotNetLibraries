namespace System.Configuration.Install
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class InstallException : SystemException
    {
        public InstallException()
        {
            base.HResult = -2146232057;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstallException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected InstallException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstallException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

