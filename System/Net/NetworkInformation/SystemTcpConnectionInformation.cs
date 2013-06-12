namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;

    internal class SystemTcpConnectionInformation : TcpConnectionInformation
    {
        private IPEndPoint localEndPoint;
        private IPEndPoint remoteEndPoint;
        private TcpState state;

        internal SystemTcpConnectionInformation(MibTcp6RowOwnerPid row)
        {
            this.state = row.state;
            int port = (row.localPort1 << 8) | row.localPort2;
            int num2 = (this.state == TcpState.Listen) ? 0 : ((row.remotePort1 << 8) | row.remotePort2);
            this.localEndPoint = new IPEndPoint(new IPAddress(row.localAddr, (long) row.localScopeId), port);
            this.remoteEndPoint = new IPEndPoint(new IPAddress(row.remoteAddr, (long) row.remoteScopeId), num2);
        }

        internal SystemTcpConnectionInformation(MibTcpRow row)
        {
            this.state = row.state;
            int port = (row.localPort1 << 8) | row.localPort2;
            int num2 = (this.state == TcpState.Listen) ? 0 : ((row.remotePort1 << 8) | row.remotePort2);
            this.localEndPoint = new IPEndPoint((long) row.localAddr, port);
            this.remoteEndPoint = new IPEndPoint((long) row.remoteAddr, num2);
        }

        public override IPEndPoint LocalEndPoint
        {
            get
            {
                return this.localEndPoint;
            }
        }

        public override IPEndPoint RemoteEndPoint
        {
            get
            {
                return this.remoteEndPoint;
            }
        }

        public override TcpState State
        {
            get
            {
                return this.state;
            }
        }
    }
}

