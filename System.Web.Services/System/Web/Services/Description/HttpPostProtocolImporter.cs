namespace System.Web.Services.Description
{
    using System;
    using System.Web.Services;
    using System.Web.Services.Protocols;

    internal class HttpPostProtocolImporter : HttpProtocolImporter
    {
        public HttpPostProtocolImporter() : base(true)
        {
        }

        protected override bool IsBindingSupported()
        {
            HttpBinding binding = (HttpBinding) base.Binding.Extensions.Find(typeof(HttpBinding));
            if (binding == null)
            {
                return false;
            }
            if (binding.Verb != "POST")
            {
                return false;
            }
            return true;
        }

        internal override Type BaseClass
        {
            get
            {
                if (base.Style == ServiceDescriptionImportStyle.Client)
                {
                    return typeof(HttpPostClientProtocol);
                }
                return typeof(WebService);
            }
        }

        public override string ProtocolName
        {
            get
            {
                return "HttpPost";
            }
        }
    }
}

