namespace System.ServiceModel.Activation
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;

    internal class MetabaseSettingsCassini : MetabaseSettings
    {
        internal MetabaseSettingsCassini(HostedHttpRequestAsyncResult result)
        {
            if (!ServiceHostingEnvironment.IsSimpleApplicationHost)
            {
                throw Fx.AssertAndThrowFatal("MetabaseSettingsCassini..ctor() Not a simple application host.");
            }
            string str = string.Format(CultureInfo.InvariantCulture, ":{0}:{1}", new object[] { result.OriginalRequestUri.Port.ToString(NumberFormatInfo.InvariantInfo), "localhost" });
            base.Bindings.Add(result.OriginalRequestUri.Scheme, new string[] { str });
            base.Protocols.Add(result.OriginalRequestUri.Scheme);
        }

        internal override HttpAccessSslFlags GetAccessSslFlags(string virtualPath)
        {
            return HttpAccessSslFlags.None;
        }

        internal override AuthenticationSchemes GetAuthenticationSchemes(string virtualPath)
        {
            return (AuthenticationSchemes.Anonymous | AuthenticationSchemes.Ntlm);
        }

        internal override ExtendedProtectionPolicy GetExtendedProtectionPolicy(string virtualPath)
        {
            return null;
        }

        internal override string GetRealm(string virtualPath)
        {
            return string.Empty;
        }
    }
}

