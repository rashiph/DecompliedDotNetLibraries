namespace System.Data.Design
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class InternalException : Exception, ISerializable
    {
        private int errorCode;
        private const string internalExceptionMessageID = "ERR_INTERNAL";
        private string internalMessage;
        private bool showErrorMesageOnReport;

        internal InternalException(string internalMessage) : this(internalMessage, (Exception) null)
        {
        }

        private InternalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.internalMessage = string.Empty;
            this.errorCode = -1;
            this.internalMessage = info.GetString("InternalMessage");
            this.errorCode = info.GetInt32("ErrorCode");
            this.showErrorMesageOnReport = info.GetBoolean("ShowErrorMesageOnReport");
        }

        internal InternalException(string internalMessage, Exception innerException) : this(innerException, internalMessage, -1, false)
        {
        }

        internal InternalException(string internalMessage, int errorCode) : this(null, internalMessage, errorCode, false)
        {
        }

        internal InternalException(string internalMessage, int errorCode, bool showTextOnReport) : this(null, internalMessage, errorCode, showTextOnReport)
        {
        }

        internal InternalException(Exception innerException, string internalMessage, int errorCode, bool showErrorMesageOnReport) : this(innerException, internalMessage, errorCode, showErrorMesageOnReport, true)
        {
        }

        internal InternalException(Exception innerException, string internalMessage, int errorCode, bool showErrorMesageOnReport, bool needAssert) : base(System.Design.SR.GetString("ERR_INTERNAL"), innerException)
        {
            this.internalMessage = string.Empty;
            this.errorCode = -1;
            this.errorCode = errorCode;
            this.showErrorMesageOnReport = showErrorMesageOnReport;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("InternalMessage", this.internalMessage);
            info.AddValue("ErrorCode", this.errorCode);
            info.AddValue("ShowErrorMesageOnReport", this.showErrorMesageOnReport);
            base.GetObjectData(info, context);
        }
    }
}

