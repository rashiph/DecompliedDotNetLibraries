namespace System.Net.NetworkInformation
{
    using System;

    public abstract class NetworkInterface
    {
        protected NetworkInterface()
        {
        }

        public static NetworkInterface[] GetAllNetworkInterfaces()
        {
            new NetworkInformationPermission(NetworkInformationAccess.Read).Demand();
            return SystemNetworkInterface.GetNetworkInterfaces();
        }

        public abstract IPInterfaceProperties GetIPProperties();
        public abstract IPv4InterfaceStatistics GetIPv4Statistics();
        public static bool GetIsNetworkAvailable()
        {
            return SystemNetworkInterface.InternalGetIsNetworkAvailable();
        }

        public abstract PhysicalAddress GetPhysicalAddress();
        public abstract bool Supports(NetworkInterfaceComponent networkInterfaceComponent);

        public abstract string Description { get; }

        public abstract string Id { get; }

        public abstract bool IsReceiveOnly { get; }

        public static int LoopbackInterfaceIndex
        {
            get
            {
                return SystemNetworkInterface.InternalLoopbackInterfaceIndex;
            }
        }

        public abstract string Name { get; }

        public abstract System.Net.NetworkInformation.NetworkInterfaceType NetworkInterfaceType { get; }

        public abstract System.Net.NetworkInformation.OperationalStatus OperationalStatus { get; }

        public abstract long Speed { get; }

        public abstract bool SupportsMulticast { get; }
    }
}

