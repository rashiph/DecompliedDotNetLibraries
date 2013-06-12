namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class MultipleSocketMultipleConnectAsync : MultipleConnectAsync
    {
        private Socket socket4;
        private Socket socket6;

        public MultipleSocketMultipleConnectAsync(SocketType socketType, ProtocolType protocolType)
        {
            if (Socket.OSSupportsIPv4)
            {
                this.socket4 = new Socket(AddressFamily.InterNetwork, socketType, protocolType);
            }
            if (Socket.OSSupportsIPv6)
            {
                this.socket6 = new Socket(AddressFamily.InterNetworkV6, socketType, protocolType);
            }
        }

        protected override IPAddress GetNextAddress(out Socket attemptSocket)
        {
            IPAddress address = null;
            attemptSocket = null;
            while (attemptSocket == null)
            {
                if (base.nextAddress >= base.addressList.Length)
                {
                    return null;
                }
                address = base.addressList[base.nextAddress];
                base.nextAddress++;
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    attemptSocket = this.socket6;
                }
                else if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    attemptSocket = this.socket4;
                }
            }
            return address;
        }

        protected override void OnFail(bool abortive)
        {
            if (this.socket4 != null)
            {
                this.socket4.Close();
            }
            if (this.socket6 != null)
            {
                this.socket6.Close();
            }
        }

        protected override void OnSucceed()
        {
            if ((this.socket4 != null) && !this.socket4.Connected)
            {
                this.socket4.Close();
            }
            if ((this.socket6 != null) && !this.socket6.Connected)
            {
                this.socket6.Close();
            }
        }
    }
}

