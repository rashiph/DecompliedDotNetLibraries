namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable, SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    internal class RuleSyntaxException : SystemException
    {
        private int errorNumber;
        private int position;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal RuleSyntaxException()
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        private RuleSyntaxException(SerializationInfo serializeInfo, StreamingContext context) : base(serializeInfo, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal RuleSyntaxException(int errorNumber, string message, int position) : base(message)
        {
            this.errorNumber = errorNumber;
            this.position = position;
        }

        internal int ErrorNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.errorNumber;
            }
        }

        internal int Position
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.position;
            }
        }
    }
}

