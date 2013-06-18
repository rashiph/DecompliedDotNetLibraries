namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class TextReturnReader : MimeReturnReader
    {
        private System.Web.Services.Protocols.PatternMatcher matcher;

        public override object GetInitializer(LogicalMethodInfo methodInfo)
        {
            return new System.Web.Services.Protocols.PatternMatcher(methodInfo.ReturnType);
        }

        public override void Initialize(object o)
        {
            this.matcher = (System.Web.Services.Protocols.PatternMatcher) o;
        }

        public override object Read(WebResponse response, Stream responseStream)
        {
            object obj2;
            try
            {
                string text = RequestResponseUtils.ReadResponse(response);
                obj2 = this.matcher.Match(text);
            }
            finally
            {
                response.Close();
            }
            return obj2;
        }
    }
}

