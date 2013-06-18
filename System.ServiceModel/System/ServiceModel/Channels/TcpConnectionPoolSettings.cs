namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    public sealed class TcpConnectionPoolSettings
    {
        private string groupName;
        private TimeSpan idleTimeout;
        private TimeSpan leaseTimeout;
        private int maxOutboundConnectionsPerEndpoint;

        internal TcpConnectionPoolSettings()
        {
            this.groupName = "default";
            this.idleTimeout = ConnectionOrientedTransportDefaults.IdleTimeout;
            this.leaseTimeout = TcpTransportDefaults.ConnectionLeaseTimeout;
            this.maxOutboundConnectionsPerEndpoint = 10;
        }

        internal TcpConnectionPoolSettings(TcpConnectionPoolSettings tcp)
        {
            this.groupName = tcp.groupName;
            this.idleTimeout = tcp.idleTimeout;
            this.leaseTimeout = tcp.leaseTimeout;
            this.maxOutboundConnectionsPerEndpoint = tcp.maxOutboundConnectionsPerEndpoint;
        }

        internal TcpConnectionPoolSettings Clone()
        {
            return new TcpConnectionPoolSettings(this);
        }

        internal bool IsMatch(TcpConnectionPoolSettings tcp)
        {
            if (this.groupName != tcp.groupName)
            {
                return false;
            }
            if (this.idleTimeout != tcp.idleTimeout)
            {
                return false;
            }
            if (this.leaseTimeout != tcp.leaseTimeout)
            {
                return false;
            }
            if (this.maxOutboundConnectionsPerEndpoint != tcp.maxOutboundConnectionsPerEndpoint)
            {
                return false;
            }
            return true;
        }

        public string GroupName
        {
            get
            {
                return this.groupName;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.groupName = value;
            }
        }

        public TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.idleTimeout = value;
            }
        }

        public TimeSpan LeaseTimeout
        {
            get
            {
                return this.leaseTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.leaseTimeout = value;
            }
        }

        public int MaxOutboundConnectionsPerEndpoint
        {
            get
            {
                return this.maxOutboundConnectionsPerEndpoint;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.maxOutboundConnectionsPerEndpoint = value;
            }
        }
    }
}

