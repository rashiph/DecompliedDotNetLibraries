namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class WorkflowMarkupSerializationException : Exception
    {
        private int columnNumber;
        private int lineNumber;

        public WorkflowMarkupSerializationException()
        {
            this.lineNumber = -1;
            this.columnNumber = -1;
        }

        public WorkflowMarkupSerializationException(string message) : base(message)
        {
            this.lineNumber = -1;
            this.columnNumber = -1;
        }

        protected WorkflowMarkupSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.lineNumber = -1;
            this.columnNumber = -1;
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.lineNumber = info.GetInt32("lineNumber");
            this.columnNumber = info.GetInt32("columnNumber");
        }

        public WorkflowMarkupSerializationException(string message, Exception innerException) : base(message, innerException)
        {
            this.lineNumber = -1;
            this.columnNumber = -1;
        }

        public WorkflowMarkupSerializationException(string message, int lineNumber, int columnNumber) : base(message)
        {
            this.lineNumber = -1;
            this.columnNumber = -1;
            this.lineNumber = lineNumber;
            this.columnNumber = columnNumber;
        }

        public WorkflowMarkupSerializationException(string message, Exception innerException, int lineNumber, int columnNumber) : base(message, innerException)
        {
            this.lineNumber = -1;
            this.columnNumber = -1;
            this.lineNumber = lineNumber;
            this.columnNumber = columnNumber;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("lineNumber", this.lineNumber, typeof(int));
            info.AddValue("columnNumber", this.columnNumber, typeof(int));
        }

        public int LineNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.lineNumber;
            }
        }

        public int LinePosition
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.columnNumber;
            }
        }
    }
}

