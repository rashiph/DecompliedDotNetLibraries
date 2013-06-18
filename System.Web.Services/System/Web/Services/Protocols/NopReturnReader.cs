namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Net;

    public class NopReturnReader : MimeReturnReader
    {
        public override object GetInitializer(LogicalMethodInfo methodInfo)
        {
            return this;
        }

        public override void Initialize(object initializer)
        {
        }

        public override object Read(WebResponse response, Stream responseStream)
        {
            response.Close();
            return null;
        }
    }
}

