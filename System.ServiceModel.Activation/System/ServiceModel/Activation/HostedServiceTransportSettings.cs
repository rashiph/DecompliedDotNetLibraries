namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Authentication.ExtendedProtection;

    internal class HostedServiceTransportSettings
    {
        public HttpAccessSslFlags AccessSslFlags;
        public System.ServiceModel.Activation.AuthFlags AuthFlags;
        public string[] AuthProviders = MetabaseSettingsIis.DefaultAuthProviders;
        public string Realm = string.Empty;

        public ExtendedProtectionPolicy IisExtendedProtectionPolicy { get; set; }
    }
}

