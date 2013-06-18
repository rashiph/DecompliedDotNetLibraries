namespace System.ServiceModel.Transactions
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.Recovery;
    using System;
    using System.IO;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.ComIntegration;
    using System.Transactions;

    internal class WsatConfiguration
    {
        private static readonly string DisabledRegistrationPath = ("WsatService/" + BindingStrings.RegistrationCoordinatorSuffix(ProtocolVersion.Version10) + "Disabled/");
        private bool inboundEnabled;
        private bool issuedTokensEnabled;
        private EndpointAddress localActivationService10;
        private EndpointAddress localActivationService11;
        private TimeSpan maxTimeout;
        private bool oleTxUpgradeEnabled;
        private const bool OleTxUpgradeEnabledDefault = true;
        private const string OleTxUpgradeEnabledValue = "OleTxUpgradeEnabled";
        private bool protocolService10Enabled;
        private bool protocolService11Enabled;
        private Uri registrationServiceAddress10;
        private Uri registrationServiceAddress11;
        private EndpointAddress remoteActivationService10;
        private EndpointAddress remoteActivationService11;
        private const string WsatKey = @"Software\Microsoft\WSAT\3.0";

        public WsatConfiguration()
        {
            WhereaboutsReader whereabouts = this.GetWhereabouts();
            ProtocolInformationReader protocolInformation = whereabouts.ProtocolInformation;
            if (protocolInformation != null)
            {
                this.protocolService10Enabled = protocolInformation.IsV10Enabled;
                this.protocolService11Enabled = protocolInformation.IsV11Enabled;
            }
            this.Initialize(whereabouts);
            this.oleTxUpgradeEnabled = ReadFlag(@"Software\Microsoft\WSAT\3.0", "OleTxUpgradeEnabled", true);
        }

        private EndpointAddress CreateActivationEndpointAddress(ProtocolInformationReader protocol, string suffix, string spnIdentity, bool isRemote)
        {
            string uriSchemeHttps;
            string hostName;
            int httpsPort;
            string str3;
            if (isRemote)
            {
                uriSchemeHttps = Uri.UriSchemeHttps;
                hostName = protocol.HostName;
                httpsPort = protocol.HttpsPort;
                str3 = protocol.BasePath + "/" + suffix + "Remote/";
            }
            else
            {
                uriSchemeHttps = Uri.UriSchemeNetPipe;
                hostName = "localhost";
                httpsPort = -1;
                str3 = protocol.HostName + "/" + protocol.BasePath + "/" + suffix;
            }
            UriBuilder builder = new UriBuilder(uriSchemeHttps, hostName, httpsPort, str3);
            if (spnIdentity != null)
            {
                return new EndpointAddress(builder.Uri, EndpointIdentity.CreateSpnIdentity(spnIdentity), new AddressHeader[0]);
            }
            return new EndpointAddress(builder.Uri, new AddressHeader[0]);
        }

        public EndpointAddress CreateRegistrationService(AddressHeader refParam, ProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return new EndpointAddress(this.registrationServiceAddress10, new AddressHeader[] { refParam });

                case ProtocolVersion.Version11:
                    return new EndpointAddress(this.registrationServiceAddress11, new AddressHeader[] { refParam });
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
        }

        private WhereaboutsReader GetWhereabouts()
        {
            WhereaboutsReader reader;
            try
            {
                reader = new WhereaboutsReader(TransactionInterop.GetWhereabouts());
            }
            catch (SerializationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionManagerConfigurationException(System.ServiceModel.SR.GetString("WhereaboutsReadFailed"), exception));
            }
            return reader;
        }

        private void Initialize(WhereaboutsReader whereabouts)
        {
            try
            {
                this.InitializeForUnmarshal(whereabouts);
                this.InitializeForMarshal(whereabouts);
            }
            catch (UriFormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionManagerConfigurationException(System.ServiceModel.SR.GetString("WsatUriCreationFailed"), exception));
            }
            catch (ArgumentOutOfRangeException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionManagerConfigurationException(System.ServiceModel.SR.GetString("WsatUriCreationFailed"), exception2));
            }
        }

        private void InitializeForMarshal(WhereaboutsReader whereabouts)
        {
            ProtocolInformationReader protocolInformation = whereabouts.ProtocolInformation;
            if ((protocolInformation != null) && protocolInformation.NetworkOutboundAccess)
            {
                if (protocolInformation.IsV10Enabled)
                {
                    UriBuilder builder = new UriBuilder(Uri.UriSchemeHttps, protocolInformation.HostName, protocolInformation.HttpsPort, protocolInformation.BasePath + "/" + BindingStrings.RegistrationCoordinatorSuffix(ProtocolVersion.Version10));
                    this.registrationServiceAddress10 = builder.Uri;
                }
                if (protocolInformation.IsV11Enabled)
                {
                    UriBuilder builder2 = new UriBuilder(Uri.UriSchemeHttps, protocolInformation.HostName, protocolInformation.HttpsPort, protocolInformation.BasePath + "/" + BindingStrings.RegistrationCoordinatorSuffix(ProtocolVersion.Version11));
                    this.registrationServiceAddress11 = builder2.Uri;
                }
                this.issuedTokensEnabled = protocolInformation.IssuedTokensEnabled;
                this.maxTimeout = protocolInformation.MaxTimeout;
            }
            else
            {
                UriBuilder builder3 = new UriBuilder(Uri.UriSchemeHttps, whereabouts.HostName, 0x1bb, DisabledRegistrationPath);
                this.registrationServiceAddress10 = builder3.Uri;
                this.registrationServiceAddress11 = builder3.Uri;
                this.issuedTokensEnabled = false;
                this.maxTimeout = TimeSpan.FromMinutes(5.0);
            }
        }

        private void InitializeForUnmarshal(WhereaboutsReader whereabouts)
        {
            ProtocolInformationReader protocolInformation = whereabouts.ProtocolInformation;
            if ((protocolInformation != null) && protocolInformation.NetworkInboundAccess)
            {
                string str;
                this.inboundEnabled = true;
                bool flag = string.Compare(Environment.MachineName, protocolInformation.NodeName, StringComparison.OrdinalIgnoreCase) == 0;
                string suffix = BindingStrings.ActivationCoordinatorSuffix(ProtocolVersion.Version10);
                string str3 = BindingStrings.ActivationCoordinatorSuffix(ProtocolVersion.Version11);
                if (protocolInformation.IsClustered || (protocolInformation.NetworkClientAccess && !flag))
                {
                    if (protocolInformation.IsClustered)
                    {
                        str = null;
                    }
                    else
                    {
                        str = "host/" + protocolInformation.HostName;
                    }
                    if (protocolInformation.IsV10Enabled)
                    {
                        this.remoteActivationService10 = this.CreateActivationEndpointAddress(protocolInformation, suffix, str, true);
                    }
                    if (protocolInformation.IsV11Enabled)
                    {
                        this.remoteActivationService11 = this.CreateActivationEndpointAddress(protocolInformation, str3, str, true);
                    }
                }
                if (flag)
                {
                    str = "host/" + protocolInformation.NodeName;
                    if (protocolInformation.IsV10Enabled)
                    {
                        this.localActivationService10 = this.CreateActivationEndpointAddress(protocolInformation, suffix, str, false);
                    }
                    if (protocolInformation.IsV11Enabled)
                    {
                        this.localActivationService11 = this.CreateActivationEndpointAddress(protocolInformation, str3, str, false);
                    }
                }
            }
        }

        public bool IsDisabledRegistrationService(EndpointAddress endpoint)
        {
            return (endpoint.Uri.AbsolutePath == DisabledRegistrationPath);
        }

        public bool IsLocalRegistrationService(EndpointAddress endpoint, ProtocolVersion protocolVersion)
        {
            if (endpoint.Uri == null)
            {
                return false;
            }
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return (endpoint.Uri == this.registrationServiceAddress10);

                case ProtocolVersion.Version11:
                    return (endpoint.Uri == this.registrationServiceAddress11);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
        }

        public bool IsProtocolServiceEnabled(ProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return this.protocolService10Enabled;

                case ProtocolVersion.Version11:
                    return this.protocolService11Enabled;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
        }

        public EndpointAddress LocalActivationService(ProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return this.localActivationService10;

                case ProtocolVersion.Version11:
                    return this.localActivationService11;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
        }

        private static bool ReadFlag(string key, string value, bool defaultValue)
        {
            return (ReadInt(key, value, defaultValue ? 1 : 0) != 0);
        }

        private static int ReadInt(string key, string value, int defaultValue)
        {
            object obj2 = ReadValue(key, value);
            if ((obj2 != null) && (obj2 is int))
            {
                return (int) obj2;
            }
            return defaultValue;
        }

        private static object ReadValue(string key, string value)
        {
            object obj2;
            try
            {
                using (RegistryHandle handle = RegistryHandle.GetNativeHKLMSubkey(key, false))
                {
                    if (handle == null)
                    {
                        return null;
                    }
                    obj2 = handle.GetValue(value);
                }
            }
            catch (SecurityException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionManagerConfigurationException(System.ServiceModel.SR.GetString("WsatRegistryValueReadError", new object[] { value }), exception));
            }
            catch (IOException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionManagerConfigurationException(System.ServiceModel.SR.GetString("WsatRegistryValueReadError", new object[] { value }), exception2));
            }
            return obj2;
        }

        public EndpointAddress RemoteActivationService(ProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return this.remoteActivationService10;

                case ProtocolVersion.Version11:
                    return this.remoteActivationService11;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
        }

        public bool InboundEnabled
        {
            get
            {
                return this.inboundEnabled;
            }
        }

        public bool IssuedTokensEnabled
        {
            get
            {
                return this.issuedTokensEnabled;
            }
        }

        public TimeSpan MaxTimeout
        {
            get
            {
                return this.maxTimeout;
            }
        }

        public bool OleTxUpgradeEnabled
        {
            get
            {
                return this.oleTxUpgradeEnabled;
            }
        }
    }
}

