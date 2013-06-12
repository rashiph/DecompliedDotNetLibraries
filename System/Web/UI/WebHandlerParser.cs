namespace System.Web.UI
{
    using System;
    using System.Web;

    internal class WebHandlerParser : SimpleWebHandlerParser
    {
        internal WebHandlerParser(string virtualPath) : base(null, virtualPath, null)
        {
        }

        internal override void ValidateBaseType(Type t)
        {
            Util.CheckAssignableType(typeof(IHttpHandler), t);
        }

        protected override string DefaultDirectiveName
        {
            get
            {
                return "webhandler";
            }
        }
    }
}

