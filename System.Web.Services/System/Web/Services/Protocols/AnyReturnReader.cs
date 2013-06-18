namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Net;

    public class AnyReturnReader : MimeReturnReader
    {
        public override object GetInitializer(LogicalMethodInfo methodInfo)
        {
            if (methodInfo.IsVoid)
            {
                return null;
            }
            return this;
        }

        public override void Initialize(object o)
        {
        }

        public override object Read(WebResponse response, Stream responseStream)
        {
            return responseStream;
        }
    }
}

