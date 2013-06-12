namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    internal class SystemIPGlobalStatistics : IPGlobalStatistics
    {
        private MibIpStats stats;

        private SystemIPGlobalStatistics()
        {
            this.stats = new MibIpStats();
        }

        internal SystemIPGlobalStatistics(AddressFamily family)
        {
            uint ipStatistics;
            this.stats = new MibIpStats();
            if (!ComNetOS.IsPostWin2K)
            {
                if (family != AddressFamily.InterNetwork)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
                }
                ipStatistics = UnsafeNetInfoNativeMethods.GetIpStatistics(out this.stats);
            }
            else
            {
                ipStatistics = UnsafeNetInfoNativeMethods.GetIpStatisticsEx(out this.stats, family);
            }
            if (ipStatistics != 0)
            {
                throw new NetworkInformationException((int) ipStatistics);
            }
        }

        public override int DefaultTtl
        {
            get
            {
                return (int) this.stats.defaultTtl;
            }
        }

        public override bool ForwardingEnabled
        {
            get
            {
                return this.stats.forwardingEnabled;
            }
        }

        public override int NumberOfInterfaces
        {
            get
            {
                return (int) this.stats.interfaces;
            }
        }

        public override int NumberOfIPAddresses
        {
            get
            {
                return (int) this.stats.ipAddresses;
            }
        }

        public override int NumberOfRoutes
        {
            get
            {
                return (int) this.stats.routes;
            }
        }

        public override long OutputPacketRequests
        {
            get
            {
                return (long) this.stats.packetOutputRequests;
            }
        }

        public override long OutputPacketRoutingDiscards
        {
            get
            {
                return (long) this.stats.outputPacketRoutingDiscards;
            }
        }

        public override long OutputPacketsDiscarded
        {
            get
            {
                return (long) this.stats.outputPacketsDiscarded;
            }
        }

        public override long OutputPacketsWithNoRoute
        {
            get
            {
                return (long) this.stats.outputPacketsWithNoRoute;
            }
        }

        public override long PacketFragmentFailures
        {
            get
            {
                return (long) this.stats.packetsFragmentFailed;
            }
        }

        public override long PacketReassembliesRequired
        {
            get
            {
                return (long) this.stats.packetsReassemblyRequired;
            }
        }

        public override long PacketReassemblyFailures
        {
            get
            {
                return (long) this.stats.packetsReassemblyFailed;
            }
        }

        public override long PacketReassemblyTimeout
        {
            get
            {
                return (long) this.stats.packetReassemblyTimeout;
            }
        }

        public override long PacketsFragmented
        {
            get
            {
                return (long) this.stats.packetsFragmented;
            }
        }

        public override long PacketsReassembled
        {
            get
            {
                return (long) this.stats.packetsReassembled;
            }
        }

        public override long ReceivedPackets
        {
            get
            {
                return (long) this.stats.packetsReceived;
            }
        }

        public override long ReceivedPacketsDelivered
        {
            get
            {
                return (long) this.stats.receivedPacketsDelivered;
            }
        }

        public override long ReceivedPacketsDiscarded
        {
            get
            {
                return (long) this.stats.receivedPacketsDiscarded;
            }
        }

        public override long ReceivedPacketsForwarded
        {
            get
            {
                return (long) this.stats.packetsForwarded;
            }
        }

        public override long ReceivedPacketsWithAddressErrors
        {
            get
            {
                return (long) this.stats.receivedPacketsWithAddressErrors;
            }
        }

        public override long ReceivedPacketsWithHeadersErrors
        {
            get
            {
                return (long) this.stats.receivedPacketsWithHeaderErrors;
            }
        }

        public override long ReceivedPacketsWithUnknownProtocol
        {
            get
            {
                return (long) this.stats.receivedPacketsWithUnknownProtocols;
            }
        }
    }
}

