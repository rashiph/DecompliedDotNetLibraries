namespace System.Net.Mail
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct LineInfo
    {
        private string line;
        private SmtpStatusCode statusCode;
        internal LineInfo(SmtpStatusCode statusCode, string line)
        {
            this.statusCode = statusCode;
            this.line = line;
        }

        internal string Line
        {
            get
            {
                return this.line;
            }
        }
        internal SmtpStatusCode StatusCode
        {
            get
            {
                return this.statusCode;
            }
        }
    }
}

