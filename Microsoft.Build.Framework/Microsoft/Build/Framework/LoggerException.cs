namespace Microsoft.Build.Framework
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class LoggerException : Exception
    {
        private string errorCode;
        private string helpKeyword;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public LoggerException()
        {
        }

        public LoggerException(string message) : base(message, null)
        {
        }

        protected LoggerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.errorCode = info.GetString("errorCode");
            this.helpKeyword = info.GetString("helpKeyword");
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public LoggerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public LoggerException(string message, Exception innerException, string errorCode, string helpKeyword) : this(message, innerException)
        {
            this.errorCode = errorCode;
            this.helpKeyword = helpKeyword;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("errorCode", this.errorCode);
            info.AddValue("helpKeyword", this.helpKeyword);
        }

        public string ErrorCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.errorCode;
            }
        }

        public string HelpKeyword
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.helpKeyword;
            }
        }
    }
}

