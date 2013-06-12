namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    internal class SystemTcpStatistics : TcpStatistics
    {
        private MibTcpStats stats;

        private SystemTcpStatistics()
        {
        }

        internal SystemTcpStatistics(AddressFamily family)
        {
            uint tcpStatistics;
            if (!ComNetOS.IsPostWin2K)
            {
                if (family != AddressFamily.InterNetwork)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
                }
                tcpStatistics = UnsafeNetInfoNativeMethods.GetTcpStatistics(out this.stats);
            }
            else
            {
                tcpStatistics = UnsafeNetInfoNativeMethods.GetTcpStatisticsEx(out this.stats, family);
            }
            if (tcpStatistics != 0)
            {
                throw new NetworkInformationException((int) tcpStatistics);
            }
        }

        public override long ConnectionsAccepted
        {
            get
            {
                return (long) this.stats.passiveOpens;
            }
        }

        public override long ConnectionsInitiated
        {
            get
            {
                return (long) this.stats.activeOpens;
            }
        }

        public override long CumulativeConnections
        {
            get
            {
                return (long) this.stats.cumulativeConnections;
            }
        }

        public override long CurrentConnections
        {
            get
            {
                return (long) this.stats.currentConnections;
            }
        }

        public override long ErrorsReceived
        {
            get
            {
                return (long) this.stats.errorsReceived;
            }
        }

        public override long FailedConnectionAttempts
        {
            get
            {
                return (long) this.stats.failedConnectionAttempts;
            }
        }

        public override long MaximumConnections
        {
            get
            {
                return (long) this.stats.maximumConnections;
            }
        }

        public override long MaximumTransmissionTimeout
        {
            get
            {
                return (long) this.stats.maximumRetransmissionTimeOut;
            }
        }

        public override long MinimumTransmissionTimeout
        {
            get
            {
                return (long) this.stats.minimumRetransmissionTimeOut;
            }
        }

        public override long ResetConnections
        {
            get
            {
                return (long) this.stats.resetConnections;
            }
        }

        public override long ResetsSent
        {
            get
            {
                return (long) this.stats.segmentsSentWithReset;
            }
        }

        public override long SegmentsReceived
        {
            get
            {
                return (long) this.stats.segmentsReceived;
            }
        }

        public override long SegmentsResent
        {
            get
            {
                return (long) this.stats.segmentsResent;
            }
        }

        public override long SegmentsSent
        {
            get
            {
                return (long) this.stats.segmentsSent;
            }
        }
    }
}

