namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class SingleSocketMultipleConnectAsync : MultipleConnectAsync
    {
        private Socket socket;
        private bool userSocket;

        public SingleSocketMultipleConnectAsync(Socket socket, bool userSocket)
        {
            this.socket = socket;
            this.userSocket = userSocket;
        }

        protected override IPAddress GetNextAddress(out Socket attemptSocket)
        {
            attemptSocket = this.socket;
            IPAddress address = null;
            do
            {
                if (base.nextAddress >= base.addressList.Length)
                {
                    return null;
                }
                address = base.addressList[base.nextAddress];
                base.nextAddress++;
            }
            while (address.AddressFamily != this.socket.AddressFamily);
            return address;
        }

        protected override void OnFail(bool abortive)
        {
            if (abortive || !this.userSocket)
            {
                this.socket.Close();
            }
        }

        protected override void OnSucceed()
        {
        }
    }
}

