namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Bridge.Configuration;
    using Microsoft.Transactions.Wsat.Clusters;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;

    internal class Configuration
    {
        private const string CommitInitialDelayValue = "CommitInitialDelay";
        private const string CommitIntervalIncreasePercentageValue = "CommitIntervalIncreasePercentage";
        private const string CommitMaxNotificationIntervalValue = "CommitMaxNotificationInterval";
        private const string CommitMaxNotificationsValue = "CommitMaxNotifications";
        private const string CommitNotificationIntervalValue = "CommitNotificationInterval";
        private TimerPolicy commitPolicy;
        public const int DefaultHttpsPort = 0x944;
        private TimeSpan defaultTimeout = new TimeSpan(0, 1, 0);
        private const string DefaultTimeoutValue = "DefaultTimeout";
        private SourceLevels diagnosticTraceLevel = SourceLevels.Warning;
        private const string DiagnosticTracingActivityTracingValue = "DiagnosticTracingActivityTracing";
        private const string DiagnosticTracingLevelValue = "DiagnosticTracing";
        private const string DiagnosticTracingPropagateActivityValue = "DiagnosticTracingPropagateActivity";
        private const string DiagnosticTracingTracePIIValue = "DiagnosticTracingTracePII";
        private const string HttpsPortValue = "HttpsPort";
        private const string IssuedTokensEnabledValue = "IssuedTokensEnabled";
        private const string KerberosGlobalAclValue = "KerberosGlobalAcl";
        private TimeSpan maxTimeout = new TimeSpan(0, 10, 0);
        private const string MaxTimeoutValue = "MaxTimeout";
        private const string OperationTimeoutValue = "OperationTimeout";
        private WSTransactionSection overrideSection;
        private CoordinationServiceConfiguration portConfig;
        private const string PreparedInitialDelayValue = "PreparedInitialDelay";
        private const string PreparedIntervalIncreasePercentageValue = "PreparedIntervalIncreasePercentage";
        private const string PreparedMaxNotificationIntervalValue = "PreparedMaxNotificationInterval";
        private const string PreparedMaxNotificationsValue = "PreparedMaxNotifications";
        private const string PreparedNotificationIntervalValue = "PreparedNotificationInterval";
        private TimerPolicy preparedPolicy;
        private const string PrepareInitialDelayValue = "PrepareInitialDelay";
        private const string PrepareIntervalIncreasePercentageValue = "PrepareIntervalIncreasePercentage";
        private const string PrepareMaxNotificationIntervalValue = "PrepareMaxNotificationInterval";
        private const string PrepareMaxNotificationsValue = "PrepareMaxNotifications";
        private const string PrepareNotificationIntervalValue = "PrepareNotificationInterval";
        private TimerPolicy preparePolicy;
        private const string ReplayInitialDelayValue = "ReplayInitialDelay";
        private const string ReplayIntervalIncreasePercentageValue = "ReplayIntervalIncreasePercentage";
        private const string ReplayMaxNotificationIntervalValue = "ReplayMaxNotificationInterval";
        private const string ReplayMaxNotificationsValue = "ReplayMaxNotifications";
        private const string ReplayNotificationIntervalValue = "ReplayNotificationInterval";
        private TimerPolicy replayPolicy;
        private SourceLevels serviceModelDiagnosticTraceLevel = SourceLevels.Error;
        private const string ServiceModelDiagnosticTracingLevelValue = "ServiceModelDiagnosticTracing";
        private ProtocolState state;
        private const string TimersSubKey = "Timers";
        private bool tracePii;
        private const string VolatileOutcomeInitialDelayValue = "VolatileOutcomeDelay";
        private TimerPolicy volatileOutcomePolicy;
        private const string WsatClusterKey = @"WSATPrivate\3.0";
        private const string WsatRegistryKey = @"Software\Microsoft\WSAT\3.0";
        private const string X509CertificateIdentityValue = "X509CertificateIdentity";
        private const string X509GlobalAclValue = "X509GlobalAcl";

        public Configuration(ProtocolState state)
        {
            DebugTrace.TraceEnter(this, "Configuration");
            this.state = state;
            this.overrideSection = this.GetOverrideSectionConfiguration();
            using (ConfigurationProvider provider = this.GetConfigurationProvider())
            {
                this.ReadDiagnosticTracingConfiguration(provider);
                this.ReadTimeoutConfiguration(provider);
                this.ReadTimerPolicyConfiguration(provider);
                this.ReadPortConfiguration(provider);
            }
            this.TraceConfiguration();
            DebugTrace.TraceLeave(this, "Configuration");
        }

        private string ExtractSubjectName(X509Certificate2 identity)
        {
            string name = string.Empty;
            if ((identity.SubjectName != null) && !string.IsNullOrEmpty(identity.SubjectName.Name))
            {
                name = identity.SubjectName.Name;
            }
            return name;
        }

        private X509Certificate2 FindCertificateByThumbprint(string thumbprint)
        {
            X509Certificate2 certificate;
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);
                if (certificates.Count != 1)
                {
                    return null;
                }
                certificate = certificates[0];
            }
            finally
            {
                store.Close();
            }
            return certificate;
        }

        private string GetBasePath()
        {
            if (this.overrideSection != null)
            {
                return this.overrideSection.AddressPrefix;
            }
            return "WsatService";
        }

        private ConfigurationProvider GetConfigurationProvider()
        {
            if (this.state.TransactionManager.Settings.IsClustered)
            {
                ClusterRegistryConfigurationProvider provider = new ClusterRegistryConfigurationProvider(ClusterUtils.GetTransactionManagerClusterResource(this.state.TransactionManager.Settings.VirtualServerName, this.state.TransactionManager.Settings.ClusterResourceType));
                using (provider)
                {
                    return provider.OpenKey(@"WSATPrivate\3.0");
                }
            }
            return new RegistryConfigurationProvider(Registry.LocalMachine, @"Software\Microsoft\WSAT\3.0");
        }

        private string GetHostName()
        {
            try
            {
                string hostNameOrAddress = this.state.TransactionManager.Settings.IsClustered ? this.state.TransactionManager.Settings.VirtualServerName : string.Empty;
                return Dns.GetHostEntry(hostNameOrAddress).HostName;
            }
            catch (SocketException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                DebugTrace.Trace(TraceLevel.Warning, "Could not resolve hostname, falling back on NetBios name: {0}", exception.Message);
                return this.state.TransactionManager.Settings.VirtualServerName;
            }
        }

        private WSTransactionSection GetOverrideSectionConfiguration()
        {
            WSTransactionSection section;
            try
            {
                section = ConfigurationStrings.GetSection(ConfigurationStrings.GetSectionPath("wsTransaction")) as WSTransactionSection;
            }
            catch (ConfigurationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("ConfigurationManagerGetSectionFailed", new object[] { exception.Message }), exception));
            }
            return section;
        }

        private void ReadDiagnosticTracingConfiguration(ConfigurationProvider provider)
        {
            this.tracePii = provider.ReadInteger("DiagnosticTracingTracePII", 0) != 0;
            DebugTrace.Pii = this.tracePii;
            this.diagnosticTraceLevel = this.ReadTraceSourceLevel(provider, "DiagnosticTracing", this.diagnosticTraceLevel);
            if (this.diagnosticTraceLevel != SourceLevels.Off)
            {
                bool flag = provider.ReadInteger("DiagnosticTracingActivityTracing", 0) != 0;
                if (flag)
                {
                    this.diagnosticTraceLevel |= SourceLevels.ActivityTracing;
                }
                try
                {
                    Microsoft.Transactions.Bridge.DiagnosticUtility.InitializeTransactionSource(this.diagnosticTraceLevel);
                }
                catch (SystemException exception)
                {
                    Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                    if (DebugTrace.Warning)
                    {
                        DebugTrace.Trace(TraceLevel.Warning, "WS-AT diagnostic tracing will be disabled : {0}", exception.Message);
                    }
                    this.diagnosticTraceLevel = SourceLevels.Off;
                }
                Microsoft.Transactions.Bridge.DiagnosticUtility.Level = this.diagnosticTraceLevel;
                this.serviceModelDiagnosticTraceLevel = this.ReadTraceSourceLevel(provider, "ServiceModelDiagnosticTracing", this.serviceModelDiagnosticTraceLevel);
                if (this.serviceModelDiagnosticTraceLevel != SourceLevels.Off)
                {
                    bool propagateActivity = provider.ReadInteger("DiagnosticTracingPropagateActivity", 0) != 0;
                    if (flag)
                    {
                        this.serviceModelDiagnosticTraceLevel |= SourceLevels.ActivityTracing;
                    }
                    try
                    {
                        Microsoft.Transactions.Bridge.DiagnosticUtility.InitializeServiceModelSource(this.serviceModelDiagnosticTraceLevel, propagateActivity, this.tracePii);
                    }
                    catch (SystemException exception2)
                    {
                        Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                        if (DebugTrace.Warning)
                        {
                            DebugTrace.Trace(TraceLevel.Warning, "ServiceModel diagnostic tracing will be disabled : {0}", exception2.Message);
                        }
                        this.serviceModelDiagnosticTraceLevel = SourceLevels.Off;
                    }
                    System.ServiceModel.DiagnosticUtility.Level = this.serviceModelDiagnosticTraceLevel;
                }
            }
        }

        private void ReadPortConfiguration(ConfigurationProvider provider)
        {
            if (this.state.TransactionManager.Settings.AnyNetworkAccess && this.state.TransactionManager.Settings.NetworkTransactionAccess)
            {
                this.portConfig.X509Certificate = this.ReadX509CertificateIdentity(provider);
                if (this.portConfig.X509Certificate != null)
                {
                    this.portConfig.Mode = CoordinationServiceMode.ProtocolService;
                    this.portConfig.BasePath = this.GetBasePath();
                    this.portConfig.SupportingTokensEnabled = provider.ReadInteger("IssuedTokensEnabled", 0) != 0;
                    this.portConfig.HttpsPort = provider.ReadInteger("HttpsPort", 0x944);
                    this.portConfig.RemoteClientsEnabled = this.state.TransactionManager.Settings.NetworkClientAccess || this.state.TransactionManager.Settings.IsClustered;
                    this.portConfig.HostName = this.GetHostName();
                    string[] collection = provider.ReadMultiString("X509GlobalAcl", null);
                    if (collection != null)
                    {
                        this.portConfig.GlobalAclX509CertificateThumbprints = new List<string>(collection);
                    }
                    string[] strArray2 = provider.ReadMultiString("KerberosGlobalAcl", null);
                    if (strArray2 == null)
                    {
                        SecurityIdentifier identifier = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
                        NTAccount account = (NTAccount) identifier.Translate(typeof(NTAccount));
                        strArray2 = new string[] { account.Value };
                    }
                    this.portConfig.GlobalAclWindowsIdentities = new List<string>(strArray2);
                }
            }
        }

        private TimeSpan ReadPositiveTimeSpan(ConfigurationProvider provider, string value, TimeSpan defaultValue)
        {
            TimeSpan span = this.ReadTimeSpan(provider, value, defaultValue);
            if (span <= TimeSpan.Zero)
            {
                span = defaultValue;
            }
            return span;
        }

        private void ReadTimeoutConfiguration(ConfigurationProvider provider)
        {
            this.defaultTimeout = this.ReadPositiveTimeSpan(provider, "DefaultTimeout", this.defaultTimeout);
            TimeSpan span = this.ReadTimeSpan(provider, "MaxTimeout", this.maxTimeout);
            if (span <= TimeSpan.Zero)
            {
                this.maxTimeout = TimeSpan.Zero;
            }
            else
            {
                this.maxTimeout = span;
                if (this.defaultTimeout > this.maxTimeout)
                {
                    this.defaultTimeout = this.maxTimeout;
                }
            }
            this.portConfig.OperationTimeout = this.ReadPositiveTimeSpan(provider, "OperationTimeout", TimeSpan.Zero);
        }

        private void ReadTimerPolicyConfiguration(ConfigurationProvider rootProvider)
        {
            ConfigurationProvider provider = rootProvider.OpenKey("Timers");
            using (provider)
            {
                this.preparePolicy.InitialDelay = this.ReadPositiveTimeSpan(provider, "PrepareInitialDelay", new TimeSpan(0, 0, 15));
                this.preparePolicy.NotificationInterval = this.ReadPositiveTimeSpan(provider, "PrepareNotificationInterval", new TimeSpan(0, 0, 15));
                this.preparePolicy.IntervalIncreasePercentage = this.ReadUShort(provider, "PrepareIntervalIncreasePercentage", 0);
                this.preparePolicy.MaxNotificationInterval = this.ReadPositiveTimeSpan(provider, "PrepareMaxNotificationInterval", new TimeSpan(0, 0, 15));
                this.preparePolicy.MaxNotifications = (uint) provider.ReadInteger("PrepareMaxNotifications", 0);
                this.commitPolicy.InitialDelay = this.ReadPositiveTimeSpan(provider, "CommitInitialDelay", new TimeSpan(0, 1, 0));
                this.commitPolicy.NotificationInterval = this.ReadPositiveTimeSpan(provider, "CommitNotificationInterval", new TimeSpan(0, 0, 30));
                this.commitPolicy.IntervalIncreasePercentage = this.ReadUShort(provider, "CommitIntervalIncreasePercentage", 50);
                this.commitPolicy.MaxNotificationInterval = this.ReadPositiveTimeSpan(provider, "CommitMaxNotificationInterval", new TimeSpan(0, 5, 0));
                this.commitPolicy.MaxNotifications = (uint) provider.ReadInteger("CommitMaxNotifications", 0x19);
                this.preparedPolicy.InitialDelay = this.ReadPositiveTimeSpan(provider, "PreparedInitialDelay", new TimeSpan(0, 0, 20));
                this.preparedPolicy.NotificationInterval = this.ReadPositiveTimeSpan(provider, "PreparedNotificationInterval", new TimeSpan(0, 0, 20));
                this.preparedPolicy.IntervalIncreasePercentage = this.ReadUShort(provider, "PreparedIntervalIncreasePercentage", 50);
                this.preparedPolicy.MaxNotificationInterval = this.ReadPositiveTimeSpan(provider, "PreparedMaxNotificationInterval", new TimeSpan(0, 5, 0));
                this.preparedPolicy.MaxNotifications = (uint) provider.ReadInteger("PreparedMaxNotifications", 0);
                this.replayPolicy.InitialDelay = this.ReadPositiveTimeSpan(provider, "ReplayInitialDelay", new TimeSpan(0, 1, 0));
                this.replayPolicy.NotificationInterval = this.ReadPositiveTimeSpan(provider, "ReplayNotificationInterval", new TimeSpan(0, 0, 30));
                this.replayPolicy.IntervalIncreasePercentage = this.ReadUShort(provider, "ReplayIntervalIncreasePercentage", 50);
                this.replayPolicy.MaxNotificationInterval = this.ReadPositiveTimeSpan(provider, "ReplayMaxNotificationInterval", new TimeSpan(0, 5, 0));
                this.replayPolicy.MaxNotifications = (uint) provider.ReadInteger("ReplayMaxNotifications", 0);
                this.volatileOutcomePolicy.InitialDelay = this.ReadPositiveTimeSpan(provider, "VolatileOutcomeDelay", new TimeSpan(0, 3, 0));
                this.volatileOutcomePolicy.NotificationInterval = TimeSpan.Zero;
                this.volatileOutcomePolicy.IntervalIncreasePercentage = 0;
                this.volatileOutcomePolicy.MaxNotificationInterval = TimeSpan.Zero;
                this.volatileOutcomePolicy.MaxNotifications = 1;
            }
        }

        private TimeSpan ReadTimeSpan(ConfigurationProvider provider, string value, TimeSpan defaultValue)
        {
            return new TimeSpan(0, 0, provider.ReadInteger(value, (int) defaultValue.TotalSeconds));
        }

        private SourceLevels ReadTraceSourceLevel(ConfigurationProvider provider, string value, SourceLevels defaultValue)
        {
            SourceLevels levels = (SourceLevels) provider.ReadInteger(value, (int) defaultValue);
            SourceLevels levels2 = levels;
            if (levels2 <= SourceLevels.Warning)
            {
                switch (levels2)
                {
                    case SourceLevels.Off:
                    case SourceLevels.Critical:
                    case SourceLevels.Error:
                        return levels;

                    case 2:
                        return defaultValue;

                    case SourceLevels.Warning:
                        return levels;
                }
                return defaultValue;
            }
            if ((levels2 != SourceLevels.Information) && (levels2 != SourceLevels.Verbose))
            {
                return defaultValue;
            }
            return levels;
        }

        private ushort ReadUShort(ConfigurationProvider provider, string value, ushort defaultValue)
        {
            int num = provider.ReadInteger(value, defaultValue);
            if ((num < 0) || (num > 0xffff))
            {
                num = defaultValue;
            }
            return (ushort) num;
        }

        private X509Certificate2 ReadX509CertificateIdentity(ConfigurationProvider provider)
        {
            X509Certificate2 certificate;
            string thumbprint = provider.ReadString("X509CertificateIdentity", null);
            if (thumbprint == null)
            {
                certificate = null;
                DebugTrace.Trace(TraceLevel.Warning, "{0} value could not be read", "X509CertificateIdentity");
                return certificate;
            }
            certificate = this.FindCertificateByThumbprint(thumbprint);
            if (certificate == null)
            {
                ThumbPrintNotFoundRecord.TraceAndLog(thumbprint);
                DebugTrace.Trace(TraceLevel.Warning, "Identity certificate with thumbprint {0} could not be found", thumbprint);
                return certificate;
            }
            if (!this.ValidateIdentityCertificate(certificate))
            {
                ThumbPrintNotValidatedRecord.TraceAndLog(thumbprint);
                DebugTrace.Trace(TraceLevel.Warning, "Identity certificate with thumbprint {0} could not be validated", thumbprint);
                certificate = null;
            }
            return certificate;
        }

        private void TraceConfiguration()
        {
            if (DebugTrace.Info)
            {
                if (this.overrideSection == null)
                {
                    DebugTrace.Trace(TraceLevel.Info, "No override config section loaded");
                }
                else
                {
                    DebugTrace.Trace(TraceLevel.Info, "Override config section loaded");
                }
                DebugTrace.Trace(TraceLevel.Info, "Default timeout is {0}", this.defaultTimeout);
                DebugTrace.Trace(TraceLevel.Info, "Maximum timeout is {0}", this.maxTimeout);
                DebugTrace.Trace(TraceLevel.Info, "Operation timeout is {0}", this.portConfig.OperationTimeout);
                X509Certificate2 certificate = this.portConfig.X509Certificate;
                if (certificate != null)
                {
                    DebugTrace.Trace(TraceLevel.Info, "Network endpoints are enabled");
                    DebugTrace.Trace(TraceLevel.Info, "Host name is {0}", this.portConfig.HostName);
                    DebugTrace.Trace(TraceLevel.Info, "HTTPS port is {0}", this.portConfig.HttpsPort);
                    DebugTrace.Trace(TraceLevel.Info, "Base path is {0}", this.portConfig.BasePath);
                    DebugTrace.Trace(TraceLevel.Info, "Identity certificate SubjectName: {0}", certificate.SubjectName.Name);
                    DebugTrace.Trace(TraceLevel.Info, "Identity certificate IssuerName: {0}", certificate.IssuerName.Name);
                    DebugTrace.Trace(TraceLevel.Info, "Identity certificate Thumbprint: {0}", certificate.Thumbprint);
                    DebugTrace.Trace(TraceLevel.Info, "Identity certificate Hash: {0}", Convert.ToBase64String(certificate.GetCertHash()));
                    DebugTrace.Trace(TraceLevel.Info, "SupportingTokens are {0}", this.portConfig.SupportingTokensEnabled ? "enabled" : "disabled");
                    if (DebugTrace.Pii)
                    {
                        if (this.portConfig.GlobalAclWindowsIdentities == null)
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Global ACL contains no windows identities");
                        }
                        else
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Global ACL contains the following windows identities:");
                            foreach (string str in this.portConfig.GlobalAclWindowsIdentities)
                            {
                                DebugTrace.TracePii(TraceLevel.Info, str);
                            }
                        }
                        if (this.portConfig.GlobalAclX509CertificateThumbprints == null)
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Global ACL contains no X509 certificate thumbprints");
                        }
                        else
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Global ACL contains the following X509 certificate thumbprints:");
                            foreach (string str2 in this.portConfig.GlobalAclX509CertificateThumbprints)
                            {
                                DebugTrace.TracePii(TraceLevel.Info, str2);
                            }
                        }
                    }
                }
                if (this.diagnosticTraceLevel == SourceLevels.Off)
                {
                    DebugTrace.Trace(TraceLevel.Info, "TransactionBridge ETW tracing is disabled");
                }
                else
                {
                    DebugTrace.Trace(TraceLevel.Info, "TransactionBridge ETW tracing is enabled at {0} level", this.diagnosticTraceLevel);
                }
                if (this.serviceModelDiagnosticTraceLevel == SourceLevels.Off)
                {
                    DebugTrace.Trace(TraceLevel.Info, "ServiceModel ETW tracing is disabled");
                }
                else
                {
                    DebugTrace.Trace(TraceLevel.Info, "ServiceModel ETW tracing is enabled at {0} level", this.serviceModelDiagnosticTraceLevel);
                }
            }
        }

        private bool ValidateIdentityCertificate(X509Certificate2 identity)
        {
            string subject = this.ExtractSubjectName(identity);
            if (!identity.HasPrivateKey)
            {
                SslNoPrivateKeyRecord.TraceAndLog(subject, identity.Thumbprint);
                DebugTrace.Trace(TraceLevel.Warning, "Identity certificate with subject name {0} and thumbprint {1} does not have a private key", subject, identity.Thumbprint);
                return false;
            }
            try
            {
                AsymmetricAlgorithm privateKey = identity.PrivateKey;
            }
            catch (CryptographicException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                SslNoAccessiblePrivateKeyRecord.TraceAndLog(subject, identity.Thumbprint);
                DebugTrace.Trace(TraceLevel.Warning, "Identity certificate with subject name {0} and thumbprint {1} does not have an accessible private key", subject, identity.Thumbprint);
                return false;
            }
            X509KeyUsageExtension extension = (X509KeyUsageExtension) identity.Extensions["2.5.29.15"];
            if ((extension != null) && ((extension.KeyUsages & (X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment)) != (X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment)))
            {
                MissingNecessaryKeyUsageRecord.TraceAndLog(subject, identity.Thumbprint, X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment);
                DebugTrace.Trace(TraceLevel.Warning, "Identity certificate with subject name {0} and thumbprint {1} does not provide {2} among its KeyUsages", subject, identity.Thumbprint, (X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment).ToString());
                return false;
            }
            X509EnhancedKeyUsageExtension extension2 = (X509EnhancedKeyUsageExtension) identity.Extensions["2.5.29.37"];
            if (extension2 != null)
            {
                if (extension2.EnhancedKeyUsages["1.3.6.1.5.5.7.3.2"] == null)
                {
                    MissingNecessaryEnhancedKeyUsageRecord.TraceAndLog(subject, identity.Thumbprint, "1.3.6.1.5.5.7.3.2");
                    DebugTrace.Trace(TraceLevel.Warning, "Identity certificate with subject name {0} and thumbprint {1} does not provide {2} as one of its EnhancedKeyUsages", subject, identity.Thumbprint, "1.3.6.1.5.5.7.3.2");
                    return false;
                }
                if (extension2.EnhancedKeyUsages["1.3.6.1.5.5.7.3.1"] == null)
                {
                    MissingNecessaryEnhancedKeyUsageRecord.TraceAndLog(subject, identity.Thumbprint, "1.3.6.1.5.5.7.3.1");
                    DebugTrace.Trace(TraceLevel.Warning, "Identity certificate with subject name {0} and thumbprint {1} does not provide {2} as one of its EnhancedKeyUsages", subject, identity.Thumbprint, "1.3.6.1.5.5.7.3.1");
                    return false;
                }
            }
            if (DebugTrace.Info)
            {
                DebugTrace.Trace(TraceLevel.Info, "Identity certificate was successfully validated");
            }
            return true;
        }

        public TimerPolicy CommitPolicy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.commitPolicy;
            }
        }

        public TimeSpan DefaultTimeout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.defaultTimeout;
            }
        }

        public TimeSpan MaxTimeout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.maxTimeout;
            }
        }

        public bool NetworkEndpointsEnabled
        {
            get
            {
                return (this.portConfig.X509Certificate != null);
            }
        }

        public CoordinationServiceConfiguration PortConfiguration
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.portConfig;
            }
        }

        public TimerPolicy PreparedPolicy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.preparedPolicy;
            }
        }

        public TimerPolicy PreparePolicy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.preparePolicy;
            }
        }

        public TimerPolicy ReplayPolicy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.replayPolicy;
            }
        }

        public TimerPolicy VolatileOutcomePolicy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.volatileOutcomePolicy;
            }
        }
    }
}

