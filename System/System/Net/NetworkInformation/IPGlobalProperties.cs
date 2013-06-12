namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;

    public abstract class IPGlobalProperties
    {
        protected IPGlobalProperties()
        {
        }

        public virtual IAsyncResult BeginGetUnicastAddresses(AsyncCallback callback, object state)
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public virtual UnicastIPAddressInformationCollection EndGetUnicastAddresses(IAsyncResult asyncResult)
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public abstract TcpConnectionInformation[] GetActiveTcpConnections();
        public abstract IPEndPoint[] GetActiveTcpListeners();
        public abstract IPEndPoint[] GetActiveUdpListeners();
        public abstract IcmpV4Statistics GetIcmpV4Statistics();
        public abstract IcmpV6Statistics GetIcmpV6Statistics();
        public static IPGlobalProperties GetIPGlobalProperties()
        {
            new NetworkInformationPermission(NetworkInformationAccess.Read).Demand();
            return new SystemIPGlobalProperties();
        }

        public abstract IPGlobalStatistics GetIPv4GlobalStatistics();
        public abstract IPGlobalStatistics GetIPv6GlobalStatistics();
        public abstract TcpStatistics GetTcpIPv4Statistics();
        public abstract TcpStatistics GetTcpIPv6Statistics();
        public abstract UdpStatistics GetUdpIPv4Statistics();
        public abstract UdpStatistics GetUdpIPv6Statistics();
        public virtual UnicastIPAddressInformationCollection GetUnicastAddresses()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        internal static IPGlobalProperties InternalGetIPGlobalProperties()
        {
            return new SystemIPGlobalProperties();
        }

        public abstract string DhcpScopeName { get; }

        public abstract string DomainName { get; }

        public abstract string HostName { get; }

        public abstract bool IsWinsProxy { get; }

        public abstract NetBiosNodeType NodeType { get; }
    }
}

