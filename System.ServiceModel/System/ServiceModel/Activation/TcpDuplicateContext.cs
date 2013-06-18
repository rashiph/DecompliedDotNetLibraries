namespace System.ServiceModel.Activation
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.Serialization;

    [KnownType(typeof(IPEndPoint)), DataContract]
    internal class TcpDuplicateContext : DuplicateContext
    {
        [DataMember]
        private System.Net.Sockets.SocketInformation socketInformation;

        public TcpDuplicateContext(System.Net.Sockets.SocketInformation socketInformation, Uri via, byte[] readData) : base(via, readData)
        {
            this.socketInformation = socketInformation;
        }

        public System.Net.Sockets.SocketInformation SocketInformation
        {
            get
            {
                return this.socketInformation;
            }
        }
    }
}

