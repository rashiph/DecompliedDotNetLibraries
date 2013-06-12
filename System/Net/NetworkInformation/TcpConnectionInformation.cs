namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;

    public abstract class TcpConnectionInformation
    {
        protected TcpConnectionInformation()
        {
        }

        public abstract IPEndPoint LocalEndPoint { get; }

        public abstract IPEndPoint RemoteEndPoint { get; }

        public abstract TcpState State { get; }
    }
}

