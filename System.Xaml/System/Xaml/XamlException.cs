namespace System.Xaml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    public class XamlException : Exception
    {
        public XamlException()
        {
        }

        public XamlException(string message) : base(message)
        {
        }

        protected XamlException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.LineNumber = info.GetInt32("Line");
            this.LinePosition = info.GetInt32("Offset");
        }

        public XamlException(string message, Exception innerException) : base(message, innerException)
        {
            XamlException exception = innerException as XamlException;
            if (exception != null)
            {
                this.LineNumber = exception.LineNumber;
                this.LinePosition = exception.LinePosition;
            }
        }

        public XamlException(string message, Exception innerException, int lineNumber, int linePosition) : base(message, innerException)
        {
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("Line", this.LineNumber);
            info.AddValue("Offset", this.LinePosition);
            base.GetObjectData(info, context);
        }

        internal void SetLineInfo(int lineNumber, int linePosition)
        {
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
        }

        public int LineNumber { get; protected set; }

        public int LinePosition { get; protected set; }

        public override string Message
        {
            get
            {
                if (this.LineNumber == 0)
                {
                    return base.Message;
                }
                if (this.LinePosition != 0)
                {
                    return System.Xaml.SR.Get("LineNumberAndPosition", new object[] { base.Message, this.LineNumber, this.LinePosition });
                }
                return System.Xaml.SR.Get("LineNumberOnly", new object[] { base.Message, this.LineNumber });
            }
        }
    }
}

