namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Text;

    public abstract class MimeParameterWriter : MimeFormatter
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected MimeParameterWriter()
        {
        }

        public virtual string GetRequestUrl(string url, object[] parameters)
        {
            return url;
        }

        public virtual void InitializeRequest(WebRequest request, object[] values)
        {
        }

        public virtual void WriteRequest(Stream requestStream, object[] values)
        {
        }

        public virtual Encoding RequestEncoding
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public virtual bool UsesWriteRequest
        {
            get
            {
                return false;
            }
        }
    }
}

