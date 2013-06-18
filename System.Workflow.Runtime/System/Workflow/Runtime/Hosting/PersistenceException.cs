namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class PersistenceException : SystemException
    {
        public PersistenceException() : base(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.PersistenceException, new object[0]))
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PersistenceException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected PersistenceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PersistenceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

