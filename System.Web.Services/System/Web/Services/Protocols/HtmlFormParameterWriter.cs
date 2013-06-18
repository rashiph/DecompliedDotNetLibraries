namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    public class HtmlFormParameterWriter : UrlEncodedParameterWriter
    {
        public override void InitializeRequest(WebRequest request, object[] values)
        {
            request.ContentType = ContentType.Compose("application/x-www-form-urlencoded", this.RequestEncoding);
        }

        public override void WriteRequest(Stream requestStream, object[] values)
        {
            if (values.Length != 0)
            {
                TextWriter writer = new StreamWriter(requestStream, new ASCIIEncoding());
                base.Encode(writer, values);
                writer.Flush();
            }
        }

        public override bool UsesWriteRequest
        {
            get
            {
                return true;
            }
        }
    }
}

