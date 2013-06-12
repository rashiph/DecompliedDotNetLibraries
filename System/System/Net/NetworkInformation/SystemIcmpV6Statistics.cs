namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    internal class SystemIcmpV6Statistics : IcmpV6Statistics
    {
        private MibIcmpInfoEx stats;

        internal SystemIcmpV6Statistics()
        {
            if (!ComNetOS.IsPostWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
            }
            uint icmpStatisticsEx = UnsafeNetInfoNativeMethods.GetIcmpStatisticsEx(out this.stats, AddressFamily.InterNetworkV6);
            if (icmpStatisticsEx != 0)
            {
                throw new NetworkInformationException((int) icmpStatisticsEx);
            }
        }

        public override long DestinationUnreachableMessagesReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 1L)];
            }
        }

        public override long DestinationUnreachableMessagesSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 1L)];
            }
        }

        public override long EchoRepliesReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 0x81L)];
            }
        }

        public override long EchoRepliesSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 0x81L)];
            }
        }

        public override long EchoRequestsReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 0x80L)];
            }
        }

        public override long EchoRequestsSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 0x80L)];
            }
        }

        public override long ErrorsReceived
        {
            get
            {
                return (long) this.stats.inStats.dwErrors;
            }
        }

        public override long ErrorsSent
        {
            get
            {
                return (long) this.stats.outStats.dwErrors;
            }
        }

        public override long MembershipQueriesReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 130L)];
            }
        }

        public override long MembershipQueriesSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 130L)];
            }
        }

        public override long MembershipReductionsReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 0x84L)];
            }
        }

        public override long MembershipReductionsSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 0x84L)];
            }
        }

        public override long MembershipReportsReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 0x83L)];
            }
        }

        public override long MembershipReportsSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 0x83L)];
            }
        }

        public override long MessagesReceived
        {
            get
            {
                return (long) this.stats.inStats.dwMsgs;
            }
        }

        public override long MessagesSent
        {
            get
            {
                return (long) this.stats.outStats.dwMsgs;
            }
        }

        public override long NeighborAdvertisementsReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 0x88L)];
            }
        }

        public override long NeighborAdvertisementsSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 0x88L)];
            }
        }

        public override long NeighborSolicitsReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 0x87L)];
            }
        }

        public override long NeighborSolicitsSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 0x87L)];
            }
        }

        public override long PacketTooBigMessagesReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 2L)];
            }
        }

        public override long PacketTooBigMessagesSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 2L)];
            }
        }

        public override long ParameterProblemsReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 4L)];
            }
        }

        public override long ParameterProblemsSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 4L)];
            }
        }

        public override long RedirectsReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 0x89L)];
            }
        }

        public override long RedirectsSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 0x89L)];
            }
        }

        public override long RouterAdvertisementsReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 0x86L)];
            }
        }

        public override long RouterAdvertisementsSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 0x86L)];
            }
        }

        public override long RouterSolicitsReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 0x85L)];
            }
        }

        public override long RouterSolicitsSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 0x85L)];
            }
        }

        public override long TimeExceededMessagesReceived
        {
            get
            {
                return (long) this.stats.inStats.rgdwTypeCount[(int) ((IntPtr) 3L)];
            }
        }

        public override long TimeExceededMessagesSent
        {
            get
            {
                return (long) this.stats.outStats.rgdwTypeCount[(int) ((IntPtr) 3L)];
            }
        }
    }
}

