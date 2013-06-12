namespace System.Net.NetworkInformation
{
    using System;

    internal class SystemIPv4InterfaceStatistics : IPv4InterfaceStatistics
    {
        private MibIfRow ifRow;

        private SystemIPv4InterfaceStatistics()
        {
            this.ifRow = new MibIfRow();
        }

        internal SystemIPv4InterfaceStatistics(long index)
        {
            this.ifRow = new MibIfRow();
            this.GetIfEntry(index);
        }

        private void GetIfEntry(long index)
        {
            if (index != 0L)
            {
                this.ifRow.dwIndex = (uint) index;
                uint ifEntry = UnsafeNetInfoNativeMethods.GetIfEntry(ref this.ifRow);
                if (ifEntry != 0)
                {
                    throw new NetworkInformationException((int) ifEntry);
                }
            }
        }

        public override long BytesReceived
        {
            get
            {
                return (long) this.ifRow.dwInOctets;
            }
        }

        public override long BytesSent
        {
            get
            {
                return (long) this.ifRow.dwOutOctets;
            }
        }

        public override long IncomingPacketsDiscarded
        {
            get
            {
                return (long) this.ifRow.dwInDiscards;
            }
        }

        public override long IncomingPacketsWithErrors
        {
            get
            {
                return (long) this.ifRow.dwInErrors;
            }
        }

        public override long IncomingUnknownProtocolPackets
        {
            get
            {
                return (long) this.ifRow.dwInUnknownProtos;
            }
        }

        internal long Mtu
        {
            get
            {
                return (long) this.ifRow.dwMtu;
            }
        }

        public override long NonUnicastPacketsReceived
        {
            get
            {
                return (long) this.ifRow.dwInNUcastPkts;
            }
        }

        public override long NonUnicastPacketsSent
        {
            get
            {
                return (long) this.ifRow.dwOutNUcastPkts;
            }
        }

        internal System.Net.NetworkInformation.OperationalStatus OperationalStatus
        {
            get
            {
                switch (this.ifRow.operStatus)
                {
                    case OldOperationalStatus.NonOperational:
                        return System.Net.NetworkInformation.OperationalStatus.Down;

                    case OldOperationalStatus.Unreachable:
                        return System.Net.NetworkInformation.OperationalStatus.Down;

                    case OldOperationalStatus.Disconnected:
                        return System.Net.NetworkInformation.OperationalStatus.Dormant;

                    case OldOperationalStatus.Connecting:
                        return System.Net.NetworkInformation.OperationalStatus.Dormant;

                    case OldOperationalStatus.Connected:
                        return System.Net.NetworkInformation.OperationalStatus.Up;

                    case OldOperationalStatus.Operational:
                        return System.Net.NetworkInformation.OperationalStatus.Up;
                }
                return System.Net.NetworkInformation.OperationalStatus.Unknown;
            }
        }

        public override long OutgoingPacketsDiscarded
        {
            get
            {
                return (long) this.ifRow.dwOutDiscards;
            }
        }

        public override long OutgoingPacketsWithErrors
        {
            get
            {
                return (long) this.ifRow.dwOutErrors;
            }
        }

        public override long OutputQueueLength
        {
            get
            {
                return (long) this.ifRow.dwOutQLen;
            }
        }

        internal long Speed
        {
            get
            {
                return (long) this.ifRow.dwSpeed;
            }
        }

        public override long UnicastPacketsReceived
        {
            get
            {
                return (long) this.ifRow.dwInUcastPkts;
            }
        }

        public override long UnicastPacketsSent
        {
            get
            {
                return (long) this.ifRow.dwOutUcastPkts;
            }
        }
    }
}

