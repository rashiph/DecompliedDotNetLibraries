namespace System.ServiceModel.Activation
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Activation.Diagnostics;

    internal class MetabaseSettingsIis6 : MetabaseSettingsIis
    {
        [SecurityCritical]
        private string appAboPath;
        [SecurityCritical]
        private HostedServiceTransportSettings appTransportSettings;
        [SecurityCritical]
        private string siteAboPath;

        [SecuritySafeCritical]
        internal MetabaseSettingsIis6()
        {
            if (Iis7Helper.IsIis7)
            {
                throw Fx.AssertAndThrowFatal("MetabaseSettingsIis6 constructor must not be called when running in IIS7");
            }
            this.SetApplicationInfo();
            using (MetabaseReader reader = new MetabaseReader())
            {
                this.PopulateSiteProperties(reader);
                this.PopulateApplicationProperties(reader);
            }
        }

        [SecuritySafeCritical]
        protected override HostedServiceTransportSettings CreateTransportSettings(string relativeVirtualPath)
        {
            HostedServiceTransportSettings settings = new HostedServiceTransportSettings();
            using (MetabaseReader reader = new MetabaseReader())
            {
                settings.Realm = this.GetRealm(reader, relativeVirtualPath);
                settings.AccessSslFlags = this.GetAccessSslFlags(reader, relativeVirtualPath);
                settings.AuthFlags = this.GetAuthFlags(reader, relativeVirtualPath);
                settings.AuthProviders = this.GetAuthProviders(reader, relativeVirtualPath);
                if ((settings.AuthFlags & AuthFlags.AuthNTLM) != AuthFlags.None)
                {
                    settings.IisExtendedProtectionPolicy = this.GetExtendedProtectionPolicy();
                }
            }
            return settings;
        }

        [SecurityCritical]
        private object FindHierarchicalProperty(MetabaseReader reader, MetabasePropertyType propertyType, string startAboPath, string endAboPath, out string matchedPath)
        {
            matchedPath = null;
            while (endAboPath.Length >= startAboPath.Length)
            {
                object data = reader.GetData(endAboPath, propertyType);
                if (data != null)
                {
                    matchedPath = endAboPath;
                    return data;
                }
                int length = endAboPath.LastIndexOf('/');
                endAboPath = endAboPath.Substring(0, length);
            }
            return null;
        }

        [SecurityCritical]
        private object FindPropertyUnderAppRoot(MetabaseReader reader, MetabasePropertyType propertyType, string relativeVirtualPath)
        {
            string str;
            return this.FindPropertyUnderAppRoot(reader, propertyType, relativeVirtualPath, out str);
        }

        [SecurityCritical]
        private object FindPropertyUnderAppRoot(MetabaseReader reader, MetabasePropertyType propertyType, string relativeVirtualPath, out string matchedPath)
        {
            string str2;
            string endAboPath = this.appAboPath + relativeVirtualPath.Substring(1);
            int index = endAboPath.IndexOf('/', this.appAboPath.Length + 1);
            if (index == -1)
            {
                str2 = endAboPath;
            }
            else
            {
                str2 = endAboPath.Substring(0, index);
            }
            return this.FindHierarchicalProperty(reader, propertyType, str2, endAboPath, out matchedPath);
        }

        [SecuritySafeCritical]
        private HttpAccessSslFlags GetAccessSslFlags(MetabaseReader reader, string relativeVirtualPath)
        {
            object obj2 = this.FindPropertyUnderAppRoot(reader, MetabasePropertyType.AccessSslFlags, relativeVirtualPath);
            if (obj2 != null)
            {
                return (HttpAccessSslFlags) ((uint) obj2);
            }
            return this.appTransportSettings.AccessSslFlags;
        }

        [SecuritySafeCritical]
        private AuthFlags GetAuthFlags(MetabaseReader reader, string relativeVirtualPath)
        {
            object obj2 = this.FindPropertyUnderAppRoot(reader, MetabasePropertyType.AuthFlags, relativeVirtualPath);
            if (obj2 != null)
            {
                return (AuthFlags) ((uint) obj2);
            }
            return this.appTransportSettings.AuthFlags;
        }

        [SecuritySafeCritical]
        private string[] GetAuthProviders(MetabaseReader reader, string relativeVirtualPath)
        {
            object obj2 = this.FindPropertyUnderAppRoot(reader, MetabasePropertyType.AuthProviders, relativeVirtualPath);
            if (obj2 != null)
            {
                string[] strArray = ((string) obj2).Split(IISConstants.CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
                if ((strArray != null) && (strArray.Length > 0))
                {
                    return strArray;
                }
            }
            return this.appTransportSettings.AuthProviders;
        }

        [SecuritySafeCritical, RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\W3SVC\Parameters\ExtendedProtection")]
        private ExtendedProtectionPolicy GetExtendedProtectionPolicy()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\W3SVC\Parameters\ExtendedProtection"))
            {
                if (key != null)
                {
                    object obj2 = key.GetValue("tokenChecking");
                    object obj3 = key.GetValue("flags");
                    object obj4 = key.GetValue("spns");
                    ExtendedProtectionTokenChecking tokenChecking = (obj2 == null) ? ExtendedProtectionTokenChecking.None : ((ExtendedProtectionTokenChecking) obj2);
                    ExtendedProtectionFlags flags = (obj3 == null) ? ExtendedProtectionFlags.None : ((ExtendedProtectionFlags) obj3);
                    List<string> spnList = (obj4 == null) ? null : new List<string>(obj4 as string[]);
                    return MetabaseSettings.BuildExtendedProtectionPolicy(tokenChecking, flags, spnList);
                }
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x90008, System.ServiceModel.Activation.SR.TraceCodeWebHostNoCBTSupport, this, null);
                }
            }
            return null;
        }

        [SecuritySafeCritical]
        private string GetRealm(MetabaseReader reader, string relativeVirtualPath)
        {
            object obj2 = this.FindPropertyUnderAppRoot(reader, MetabasePropertyType.Realm, relativeVirtualPath);
            if (obj2 != null)
            {
                return (string) obj2;
            }
            return this.appTransportSettings.Realm;
        }

        [SecuritySafeCritical]
        private void PopulateApplicationProperties(MetabaseReader reader)
        {
            int num = 0;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = !base.Bindings.ContainsKey(Uri.UriSchemeHttps);
            bool flag4 = false;
            this.appTransportSettings = new HostedServiceTransportSettings();
            string appAboPath = this.appAboPath;
            object obj2 = null;
            while ((num < 4) && (appAboPath.Length >= this.siteAboPath.Length))
            {
                if (!flag && ((obj2 = reader.GetData(appAboPath, MetabasePropertyType.Realm)) != null))
                {
                    this.appTransportSettings.Realm = (string) obj2;
                    flag = true;
                    num++;
                }
                if (!flag2 && ((obj2 = reader.GetData(appAboPath, MetabasePropertyType.AuthFlags)) != null))
                {
                    this.appTransportSettings.AuthFlags = (AuthFlags) ((uint) obj2);
                    flag2 = true;
                    num++;
                }
                if (!flag3 && ((obj2 = reader.GetData(appAboPath, MetabasePropertyType.AccessSslFlags)) != null))
                {
                    this.appTransportSettings.AccessSslFlags = (HttpAccessSslFlags) ((uint) obj2);
                    flag3 = true;
                    num++;
                }
                if (!flag4 && ((obj2 = reader.GetData(appAboPath, MetabasePropertyType.AuthProviders)) != null))
                {
                    this.appTransportSettings.AuthProviders = ((string) obj2).Split(IISConstants.CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
                    flag4 = true;
                    num++;
                }
                int length = appAboPath.LastIndexOf('/');
                appAboPath = appAboPath.Substring(0, length);
            }
            if ((this.appTransportSettings.AuthProviders == null) || (this.appTransportSettings.AuthProviders.Length == 0))
            {
                this.appTransportSettings.AuthProviders = MetabaseSettingsIis.DefaultAuthProviders;
            }
        }

        [SecuritySafeCritical]
        private void PopulateSiteProperties(MetabaseReader reader)
        {
            object data = reader.GetData(this.siteAboPath, MetabasePropertyType.ServerBindings);
            if (data != null)
            {
                string[] strArray = (string[]) data;
                if (strArray.Length > 0)
                {
                    base.Bindings.Add(Uri.UriSchemeHttp, strArray);
                }
            }
            data = reader.GetData(this.siteAboPath, MetabasePropertyType.SecureBindings);
            if (data != null)
            {
                string[] strArray2 = (string[]) data;
                if (strArray2.Length > 0)
                {
                    base.Bindings.Add(Uri.UriSchemeHttps, strArray2);
                }
            }
            foreach (string str in base.Bindings.Keys)
            {
                base.Protocols.Add(str);
            }
        }

        [SecuritySafeCritical]
        private void SetApplicationInfo()
        {
            string unsafeApplicationID = HostingEnvironmentWrapper.UnsafeApplicationID;
            int index = unsafeApplicationID.IndexOf('/', "/LM/W3SVC/".Length);
            this.siteAboPath = unsafeApplicationID.Substring("/LM".Length, index - "/LM".Length);
            if (HostingEnvironmentWrapper.ApplicationVirtualPath.Length > 1)
            {
                this.appAboPath = this.siteAboPath + "/Root" + HostingEnvironmentWrapper.ApplicationVirtualPath;
            }
            else
            {
                if ((HostingEnvironmentWrapper.ApplicationVirtualPath.Length != 1) || (HostingEnvironmentWrapper.ApplicationVirtualPath[0] != '/'))
                {
                    throw Fx.AssertAndThrowFatal("ApplicationVirtualPath must be '/'.");
                }
                this.appAboPath = this.siteAboPath + "/Root";
            }
        }

        private static class IISConstants
        {
            internal const char AboPathDelimiter = '/';
            internal const string CBTRegistryHKLMPath = @"System\CurrentControlSet\Services\W3SVC\Parameters\ExtendedProtection";
            internal static char[] CommaSeparator = new char[] { ',' };
            internal const string ExtendedProtectionElementName = "extendedProtection";
            internal const string FlagsAttributeName = "flags";
            internal const string LMSegment = "/LM";
            internal const string RootSegment = "/Root";
            internal const string SpnAttributeName = "spns";
            internal const string TokenCheckingAttributeName = "tokenChecking";
        }
    }
}

