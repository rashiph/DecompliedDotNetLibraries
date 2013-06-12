namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    public class PingReply
    {
        private IPAddress address;
        private byte[] buffer;
        private IPStatus ipStatus;
        private PingOptions options;
        private long rtt;

        internal PingReply()
        {
        }

        internal PingReply(IcmpEchoReply reply)
        {
            this.address = new IPAddress((long) reply.address);
            this.ipStatus = (IPStatus) reply.status;
            if (this.ipStatus == IPStatus.Success)
            {
                this.rtt = reply.roundTripTime;
                this.buffer = new byte[reply.dataSize];
                Marshal.Copy(reply.data, this.buffer, 0, reply.dataSize);
                this.options = new PingOptions(reply.options);
            }
            else
            {
                this.buffer = new byte[0];
            }
        }

        internal PingReply(IPStatus ipStatus)
        {
            this.ipStatus = ipStatus;
            this.buffer = new byte[0];
        }

        internal PingReply(Icmp6EchoReply reply, IntPtr dataPtr, int sendSize)
        {
            this.address = new IPAddress(reply.Address.Address, (long) reply.Address.ScopeID);
            this.ipStatus = (IPStatus) reply.Status;
            if (this.ipStatus == IPStatus.Success)
            {
                this.rtt = reply.RoundTripTime;
                this.buffer = new byte[sendSize];
                Marshal.Copy(IntPtrHelper.Add(dataPtr, 0x24), this.buffer, 0, sendSize);
            }
            else
            {
                this.buffer = new byte[0];
            }
        }

        internal PingReply(byte[] data, int dataLength, IPAddress address, int time)
        {
            this.address = address;
            this.rtt = time;
            this.ipStatus = this.GetIPStatus((IcmpV4Type) data[20], (IcmpV4Code) data[0x15]);
            if (this.ipStatus == IPStatus.Success)
            {
                this.buffer = new byte[dataLength - 0x1c];
                Array.Copy(data, 0x1c, this.buffer, 0, dataLength - 0x1c);
            }
            else
            {
                this.buffer = new byte[0];
            }
        }

        private IPStatus GetIPStatus(IcmpV4Type type, IcmpV4Code code)
        {
            switch (type)
            {
                case IcmpV4Type.ICMP4_ECHO_REPLY:
                    return IPStatus.Success;

                case IcmpV4Type.ICMP4_DST_UNREACH:
                    switch (code)
                    {
                        case IcmpV4Code.ICMP4_UNREACH_NET:
                            return IPStatus.DestinationNetworkUnreachable;

                        case IcmpV4Code.ICMP4_UNREACH_HOST:
                            return IPStatus.DestinationHostUnreachable;

                        case IcmpV4Code.ICMP4_UNREACH_PROTOCOL:
                            return IPStatus.DestinationProtocolUnreachable;

                        case IcmpV4Code.ICMP4_UNREACH_PORT:
                            return IPStatus.DestinationPortUnreachable;

                        case IcmpV4Code.ICMP4_UNREACH_FRAG_NEEDED:
                            return IPStatus.PacketTooBig;
                    }
                    return IPStatus.DestinationUnreachable;

                case IcmpV4Type.ICMP4_SOURCE_QUENCH:
                    return IPStatus.SourceQuench;

                case IcmpV4Type.ICMP4_TIME_EXCEEDED:
                    return IPStatus.TtlExpired;

                case IcmpV4Type.ICMP4_PARAM_PROB:
                    return IPStatus.ParameterProblem;
            }
            return IPStatus.Unknown;
        }

        public IPAddress Address
        {
            get
            {
                return this.address;
            }
        }

        public byte[] Buffer
        {
            get
            {
                return this.buffer;
            }
        }

        public PingOptions Options
        {
            get
            {
                if (!ComNetOS.IsWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
                }
                return this.options;
            }
        }

        public long RoundtripTime
        {
            get
            {
                return this.rtt;
            }
        }

        public IPStatus Status
        {
            get
            {
                return this.ipStatus;
            }
        }
    }
}

