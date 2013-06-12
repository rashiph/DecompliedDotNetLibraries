namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct IPPacketInformation
    {
        private IPAddress address;
        private int networkInterface;
        internal IPPacketInformation(IPAddress address, int networkInterface)
        {
            this.address = address;
            this.networkInterface = networkInterface;
        }

        public IPAddress Address
        {
            get
            {
                return this.address;
            }
        }
        public int Interface
        {
            get
            {
                return this.networkInterface;
            }
        }
        public static bool operator ==(IPPacketInformation packetInformation1, IPPacketInformation packetInformation2)
        {
            return packetInformation1.Equals(packetInformation2);
        }

        public static bool operator !=(IPPacketInformation packetInformation1, IPPacketInformation packetInformation2)
        {
            return !packetInformation1.Equals(packetInformation2);
        }

        public override bool Equals(object comparand)
        {
            if (comparand == null)
            {
                return false;
            }
            if (!(comparand is IPPacketInformation))
            {
                return false;
            }
            IPPacketInformation information = (IPPacketInformation) comparand;
            return (this.address.Equals(information.address) && (this.networkInterface == information.networkInterface));
        }

        public override int GetHashCode()
        {
            return (this.address.GetHashCode() + this.networkInterface.GetHashCode());
        }
    }
}

