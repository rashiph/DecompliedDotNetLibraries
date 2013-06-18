namespace Microsoft.VisualBasic.FileIO
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public class MalformedLineException : Exception
    {
        private const string LINE_NUMBER_PROPERTY = "LineNumber";
        private long m_LineNumber;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MalformedLineException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MalformedLineException(string message) : base(message)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected MalformedLineException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info != null)
            {
                this.m_LineNumber = info.GetInt32("LineNumber");
            }
            else
            {
                this.m_LineNumber = -1L;
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MalformedLineException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MalformedLineException(string message, long lineNumber) : base(message)
        {
            this.m_LineNumber = lineNumber;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MalformedLineException(string message, long lineNumber, Exception innerException) : base(message, innerException)
        {
            this.m_LineNumber = lineNumber;
        }

        [SecurityCritical, EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue("LineNumber", this.m_LineNumber, typeof(long));
            }
            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return (base.ToString() + " " + Utils.GetResourceString("TextFieldParser_MalformedExtraData", new string[] { this.LineNumber.ToString(CultureInfo.InvariantCulture) }));
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        public long LineNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_LineNumber;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_LineNumber = value;
            }
        }
    }
}

