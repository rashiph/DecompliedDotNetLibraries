namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    internal class ExclusiveTcpListener : TcpListener
    {
        internal ExclusiveTcpListener(IPAddress localaddr, int port) : base(localaddr, port)
        {
        }

        internal void Start(bool exclusiveAddressUse)
        {
            bool flag = ((exclusiveAddressUse && (Environment.OSVersion.Platform == PlatformID.Win32NT)) && (base.Server != null)) && !base.Active;
            if (flag)
            {
                base.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, 1);
            }
            try
            {
                base.Start();
            }
            catch (SocketException)
            {
                if (!flag)
                {
                    throw;
                }
                base.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, 0);
                base.Start();
            }
        }

        internal bool IsListening
        {
            get
            {
                return base.Active;
            }
        }
    }
}

