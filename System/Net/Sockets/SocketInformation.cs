namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct SocketInformation
    {
        private byte[] protocolInformation;
        private SocketInformationOptions options;
        [OptionalField]
        private EndPoint remoteEndPoint;
        public byte[] ProtocolInformation
        {
            get
            {
                return this.protocolInformation;
            }
            set
            {
                this.protocolInformation = value;
            }
        }
        public SocketInformationOptions Options
        {
            get
            {
                return this.options;
            }
            set
            {
                this.options = value;
            }
        }
        internal bool IsNonBlocking
        {
            get
            {
                return ((this.options & SocketInformationOptions.NonBlocking) != 0);
            }
            set
            {
                if (value)
                {
                    this.options |= SocketInformationOptions.NonBlocking;
                }
                else
                {
                    this.options &= ~SocketInformationOptions.NonBlocking;
                }
            }
        }
        internal bool IsConnected
        {
            get
            {
                return ((this.options & SocketInformationOptions.Connected) != 0);
            }
            set
            {
                if (value)
                {
                    this.options |= SocketInformationOptions.Connected;
                }
                else
                {
                    this.options &= ~SocketInformationOptions.Connected;
                }
            }
        }
        internal bool IsListening
        {
            get
            {
                return ((this.options & SocketInformationOptions.Listening) != 0);
            }
            set
            {
                if (value)
                {
                    this.options |= SocketInformationOptions.Listening;
                }
                else
                {
                    this.options &= ~SocketInformationOptions.Listening;
                }
            }
        }
        internal bool UseOnlyOverlappedIO
        {
            get
            {
                return ((this.options & SocketInformationOptions.UseOnlyOverlappedIO) != 0);
            }
            set
            {
                if (value)
                {
                    this.options |= SocketInformationOptions.UseOnlyOverlappedIO;
                }
                else
                {
                    this.options &= ~SocketInformationOptions.UseOnlyOverlappedIO;
                }
            }
        }
        internal EndPoint RemoteEndPoint
        {
            get
            {
                return this.remoteEndPoint;
            }
            set
            {
                this.remoteEndPoint = value;
            }
        }
    }
}

