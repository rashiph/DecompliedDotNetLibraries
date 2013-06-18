namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public sealed class MsmqIntegrationSecurity
    {
        internal const MsmqIntegrationSecurityMode DefaultMode = MsmqIntegrationSecurityMode.Transport;
        private MsmqIntegrationSecurityMode mode = MsmqIntegrationSecurityMode.Transport;
        private MsmqTransportSecurity transportSecurity = new MsmqTransportSecurity();

        internal void ConfigureTransportSecurity(MsmqBindingElementBase msmq)
        {
            if (this.mode == MsmqIntegrationSecurityMode.Transport)
            {
                msmq.MsmqTransportSecurity = this.Transport;
            }
            else
            {
                msmq.MsmqTransportSecurity.Disable();
            }
        }

        [DefaultValue(1)]
        public MsmqIntegrationSecurityMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (!MsmqIntegrationSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public MsmqTransportSecurity Transport
        {
            get
            {
                return this.transportSecurity;
            }
            set
            {
                this.transportSecurity = value;
            }
        }
    }
}

