namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Threading;

    internal sealed class SettingsSectionInternal
    {
        private bool alwaysUseCompletionPortsForAccept;
        private bool alwaysUseCompletionPortsForConnect;
        private bool checkCertificateName;
        private bool checkCertificateRevocationList;
        private int dnsRefreshTimeout;
        private int downloadTimeout;
        private bool enableDnsRoundRobin;
        private System.Net.Security.EncryptionPolicy encryptionPolicy;
        private bool expect100Continue;
        private bool httpListenerUnescapeRequestUrl;
        private System.Net.Sockets.IPProtectionLevel ipProtectionLevel;
        private bool ipv6Enabled;
        private int maximumErrorResponseLength;
        private int maximumResponseHeadersLength;
        private int maximumUnauthorizedUploadLength;
        private bool performanceCountersEnabled;
        private static object s_InternalSyncObject = null;
        private static SettingsSectionInternal s_settings;
        private bool useNagleAlgorithm;
        private bool useUnsafeHeaderParsing;

        internal SettingsSectionInternal(SettingsSection section)
        {
            if (section == null)
            {
                section = new SettingsSection();
            }
            this.alwaysUseCompletionPortsForConnect = section.Socket.AlwaysUseCompletionPortsForConnect;
            this.alwaysUseCompletionPortsForAccept = section.Socket.AlwaysUseCompletionPortsForAccept;
            this.checkCertificateName = section.ServicePointManager.CheckCertificateName;
            this.checkCertificateRevocationList = section.ServicePointManager.CheckCertificateRevocationList;
            this.dnsRefreshTimeout = section.ServicePointManager.DnsRefreshTimeout;
            this.ipProtectionLevel = section.Socket.IPProtectionLevel;
            this.ipv6Enabled = section.Ipv6.Enabled;
            this.enableDnsRoundRobin = section.ServicePointManager.EnableDnsRoundRobin;
            this.encryptionPolicy = section.ServicePointManager.EncryptionPolicy;
            this.expect100Continue = section.ServicePointManager.Expect100Continue;
            this.maximumUnauthorizedUploadLength = section.HttpWebRequest.MaximumUnauthorizedUploadLength;
            this.maximumResponseHeadersLength = section.HttpWebRequest.MaximumResponseHeadersLength;
            this.maximumErrorResponseLength = section.HttpWebRequest.MaximumErrorResponseLength;
            this.useUnsafeHeaderParsing = section.HttpWebRequest.UseUnsafeHeaderParsing;
            this.useNagleAlgorithm = section.ServicePointManager.UseNagleAlgorithm;
            TimeSpan downloadTimeout = section.WebProxyScript.DownloadTimeout;
            this.downloadTimeout = ((downloadTimeout == TimeSpan.MaxValue) || (downloadTimeout == TimeSpan.Zero)) ? -1 : ((int) downloadTimeout.TotalMilliseconds);
            this.performanceCountersEnabled = section.PerformanceCounters.Enabled;
            this.httpListenerUnescapeRequestUrl = section.HttpListener.UnescapeRequestUrl;
        }

        internal static SettingsSectionInternal GetSection()
        {
            return new SettingsSectionInternal((SettingsSection) System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.SettingsSectionPath));
        }

        internal bool AlwaysUseCompletionPortsForAccept
        {
            get
            {
                return this.alwaysUseCompletionPortsForAccept;
            }
        }

        internal bool AlwaysUseCompletionPortsForConnect
        {
            get
            {
                return this.alwaysUseCompletionPortsForConnect;
            }
        }

        internal bool CheckCertificateName
        {
            get
            {
                return this.checkCertificateName;
            }
        }

        internal bool CheckCertificateRevocationList
        {
            get
            {
                return this.checkCertificateRevocationList;
            }
            set
            {
                this.checkCertificateRevocationList = value;
            }
        }

        internal int DnsRefreshTimeout
        {
            get
            {
                return this.dnsRefreshTimeout;
            }
            set
            {
                this.dnsRefreshTimeout = value;
            }
        }

        internal int DownloadTimeout
        {
            get
            {
                return this.downloadTimeout;
            }
        }

        internal bool EnableDnsRoundRobin
        {
            get
            {
                return this.enableDnsRoundRobin;
            }
            set
            {
                this.enableDnsRoundRobin = value;
            }
        }

        internal System.Net.Security.EncryptionPolicy EncryptionPolicy
        {
            get
            {
                return this.encryptionPolicy;
            }
        }

        internal bool Expect100Continue
        {
            get
            {
                return this.expect100Continue;
            }
            set
            {
                this.expect100Continue = value;
            }
        }

        internal bool HttpListenerUnescapeRequestUrl
        {
            get
            {
                return this.httpListenerUnescapeRequestUrl;
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal System.Net.Sockets.IPProtectionLevel IPProtectionLevel
        {
            get
            {
                return this.ipProtectionLevel;
            }
        }

        internal bool Ipv6Enabled
        {
            get
            {
                return this.ipv6Enabled;
            }
        }

        internal int MaximumErrorResponseLength
        {
            get
            {
                return this.maximumErrorResponseLength;
            }
            set
            {
                this.maximumErrorResponseLength = value;
            }
        }

        internal int MaximumResponseHeadersLength
        {
            get
            {
                return this.maximumResponseHeadersLength;
            }
            set
            {
                this.maximumResponseHeadersLength = value;
            }
        }

        internal int MaximumUnauthorizedUploadLength
        {
            get
            {
                return this.maximumUnauthorizedUploadLength;
            }
        }

        internal bool PerformanceCountersEnabled
        {
            get
            {
                return this.performanceCountersEnabled;
            }
        }

        internal static SettingsSectionInternal Section
        {
            get
            {
                if (s_settings == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (s_settings == null)
                        {
                            s_settings = new SettingsSectionInternal((SettingsSection) System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.SettingsSectionPath));
                        }
                    }
                }
                return s_settings;
            }
        }

        internal bool UseNagleAlgorithm
        {
            get
            {
                return this.useNagleAlgorithm;
            }
            set
            {
                this.useNagleAlgorithm = value;
            }
        }

        internal bool UseUnsafeHeaderParsing
        {
            get
            {
                return this.useUnsafeHeaderParsing;
            }
        }
    }
}

