namespace System.Net.NetworkInformation
{
    using System;

    internal class SystemIcmpV4Statistics : IcmpV4Statistics
    {
        private MibIcmpInfo stats;

        internal SystemIcmpV4Statistics()
        {
            uint icmpStatistics = UnsafeNetInfoNativeMethods.GetIcmpStatistics(out this.stats);
            if (icmpStatistics != 0)
            {
                throw new NetworkInformationException((int) icmpStatistics);
            }
        }

        public override long AddressMaskRepliesReceived
        {
            get
            {
                return (long) this.stats.inStats.addressMaskReplies;
            }
        }

        public override long AddressMaskRepliesSent
        {
            get
            {
                return (long) this.stats.outStats.addressMaskReplies;
            }
        }

        public override long AddressMaskRequestsReceived
        {
            get
            {
                return (long) this.stats.inStats.addressMaskRequests;
            }
        }

        public override long AddressMaskRequestsSent
        {
            get
            {
                return (long) this.stats.outStats.addressMaskRequests;
            }
        }

        public override long DestinationUnreachableMessagesReceived
        {
            get
            {
                return (long) this.stats.inStats.destinationUnreachables;
            }
        }

        public override long DestinationUnreachableMessagesSent
        {
            get
            {
                return (long) this.stats.outStats.destinationUnreachables;
            }
        }

        public override long EchoRepliesReceived
        {
            get
            {
                return (long) this.stats.inStats.echoReplies;
            }
        }

        public override long EchoRepliesSent
        {
            get
            {
                return (long) this.stats.outStats.echoReplies;
            }
        }

        public override long EchoRequestsReceived
        {
            get
            {
                return (long) this.stats.inStats.echoRequests;
            }
        }

        public override long EchoRequestsSent
        {
            get
            {
                return (long) this.stats.outStats.echoRequests;
            }
        }

        public override long ErrorsReceived
        {
            get
            {
                return (long) this.stats.inStats.errors;
            }
        }

        public override long ErrorsSent
        {
            get
            {
                return (long) this.stats.outStats.errors;
            }
        }

        public override long MessagesReceived
        {
            get
            {
                return (long) this.stats.inStats.messages;
            }
        }

        public override long MessagesSent
        {
            get
            {
                return (long) this.stats.outStats.messages;
            }
        }

        public override long ParameterProblemsReceived
        {
            get
            {
                return (long) this.stats.inStats.parameterProblems;
            }
        }

        public override long ParameterProblemsSent
        {
            get
            {
                return (long) this.stats.outStats.parameterProblems;
            }
        }

        public override long RedirectsReceived
        {
            get
            {
                return (long) this.stats.inStats.redirects;
            }
        }

        public override long RedirectsSent
        {
            get
            {
                return (long) this.stats.outStats.redirects;
            }
        }

        public override long SourceQuenchesReceived
        {
            get
            {
                return (long) this.stats.inStats.sourceQuenches;
            }
        }

        public override long SourceQuenchesSent
        {
            get
            {
                return (long) this.stats.outStats.sourceQuenches;
            }
        }

        public override long TimeExceededMessagesReceived
        {
            get
            {
                return (long) this.stats.inStats.timeExceeds;
            }
        }

        public override long TimeExceededMessagesSent
        {
            get
            {
                return (long) this.stats.outStats.timeExceeds;
            }
        }

        public override long TimestampRepliesReceived
        {
            get
            {
                return (long) this.stats.inStats.timestampReplies;
            }
        }

        public override long TimestampRepliesSent
        {
            get
            {
                return (long) this.stats.outStats.timestampReplies;
            }
        }

        public override long TimestampRequestsReceived
        {
            get
            {
                return (long) this.stats.inStats.timestampRequests;
            }
        }

        public override long TimestampRequestsSent
        {
            get
            {
                return (long) this.stats.outStats.timestampRequests;
            }
        }
    }
}

