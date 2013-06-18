namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime;

    public abstract class MimeReturnReader : MimeFormatter
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected MimeReturnReader()
        {
        }

        public abstract object Read(WebResponse response, Stream responseStream);
    }
}

