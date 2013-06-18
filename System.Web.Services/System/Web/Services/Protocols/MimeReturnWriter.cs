namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Web;

    internal abstract class MimeReturnWriter : MimeFormatter
    {
        protected MimeReturnWriter()
        {
        }

        internal abstract void Write(HttpResponse response, Stream outputStream, object returnValue);
    }
}

