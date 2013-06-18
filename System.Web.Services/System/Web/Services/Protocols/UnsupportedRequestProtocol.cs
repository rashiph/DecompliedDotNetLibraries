namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;

    internal class UnsupportedRequestProtocol : ServerProtocol
    {
        private int httpCode;

        internal UnsupportedRequestProtocol(int httpCode)
        {
            this.httpCode = httpCode;
        }

        internal override bool Initialize()
        {
            return true;
        }

        internal override object[] ReadParameters()
        {
            return new object[0];
        }

        internal override bool WriteException(Exception e, Stream outputStream)
        {
            return false;
        }

        internal override void WriteReturns(object[] returnValues, Stream outputStream)
        {
        }

        internal int HttpCode
        {
            get
            {
                return this.httpCode;
            }
        }

        internal override bool IsOneWay
        {
            get
            {
                return false;
            }
        }

        internal override LogicalMethodInfo MethodInfo
        {
            get
            {
                return null;
            }
        }

        internal override System.Web.Services.Protocols.ServerType ServerType
        {
            get
            {
                return null;
            }
        }
    }
}

