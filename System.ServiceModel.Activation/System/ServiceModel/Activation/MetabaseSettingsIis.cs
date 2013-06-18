namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.Web;

    internal abstract class MetabaseSettingsIis : MetabaseSettings
    {
        internal static string[] DefaultAuthProviders = new string[] { "negotiate", "ntlm" };
        internal const string NegotiateAuthProvider = "negotiate";
        internal const string NtlmAuthProvider = "ntlm";
        private IDictionary<string, HostedServiceTransportSettings> transportSettingsTable;

        protected MetabaseSettingsIis()
        {
            if (ServiceHostingEnvironment.IsSimpleApplicationHost)
            {
                throw Fx.AssertAndThrowFatal("MetabaseSettingsIis..ctor() Is a simple application host.");
            }
            this.transportSettingsTable = new Dictionary<string, HostedServiceTransportSettings>(StringComparer.OrdinalIgnoreCase);
        }

        protected abstract HostedServiceTransportSettings CreateTransportSettings(string relativeVirtualPath);
        internal override HttpAccessSslFlags GetAccessSslFlags(string virtualPath)
        {
            return this.GetTransportSettings(virtualPath).AccessSslFlags;
        }

        internal override AuthenticationSchemes GetAuthenticationSchemes(string virtualPath)
        {
            HostedServiceTransportSettings transportSettings = this.GetTransportSettings(virtualPath);
            return this.RemapAuthenticationSchemes(transportSettings.AuthFlags, transportSettings.AuthProviders);
        }

        internal override ExtendedProtectionPolicy GetExtendedProtectionPolicy(string virtualPath)
        {
            return this.GetTransportSettings(virtualPath).IisExtendedProtectionPolicy;
        }

        internal override string GetRealm(string virtualPath)
        {
            return this.GetTransportSettings(virtualPath).Realm;
        }

        private HostedServiceTransportSettings GetTransportSettings(string virtualPath)
        {
            HostedServiceTransportSettings settings;
            string key = VirtualPathUtility.ToAppRelative(virtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath);
            if (!this.transportSettingsTable.TryGetValue(key, out settings))
            {
                lock (this.ThisLock)
                {
                    if (!this.transportSettingsTable.TryGetValue(key, out settings))
                    {
                        settings = this.CreateTransportSettings(key);
                        this.transportSettingsTable.Add(key, settings);
                    }
                }
            }
            return settings;
        }

        private AuthenticationSchemes RemapAuthenticationSchemes(AuthFlags flags, string[] providers)
        {
            AuthenticationSchemes none = AuthenticationSchemes.None;
            if ((flags & AuthFlags.AuthAnonymous) != AuthFlags.None)
            {
                none |= AuthenticationSchemes.Anonymous;
            }
            if ((flags & AuthFlags.AuthBasic) != AuthFlags.None)
            {
                none |= AuthenticationSchemes.Basic;
            }
            if ((flags & AuthFlags.AuthMD5) != AuthFlags.None)
            {
                none |= AuthenticationSchemes.Digest;
            }
            if ((flags & AuthFlags.AuthNTLM) != AuthFlags.None)
            {
                for (int i = 0; i < providers.Length; i++)
                {
                    if (providers[i].StartsWith("negotiate", StringComparison.OrdinalIgnoreCase))
                    {
                        none |= AuthenticationSchemes.Negotiate;
                    }
                    else
                    {
                        if (string.Compare(providers[i], "ntlm", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            throw FxTrace.Exception.AsError(new NotSupportedException(System.ServiceModel.Activation.SR.Hosting_NotSupportedAuthScheme(providers[i])));
                        }
                        none |= AuthenticationSchemes.Ntlm;
                    }
                }
            }
            if ((flags & AuthFlags.AuthPassport) != AuthFlags.None)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.ServiceModel.Activation.SR.Hosting_NotSupportedAuthScheme("Passport")));
            }
            return none;
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }
    }
}

