namespace System.Net
{
    using System;
    using System.Net.Sockets;

    public class DnsEndPoint : EndPoint
    {
        private System.Net.Sockets.AddressFamily m_Family;
        private string m_Host;
        private int m_Port;

        public DnsEndPoint(string host, int port) : this(host, port, System.Net.Sockets.AddressFamily.Unspecified)
        {
        }

        public DnsEndPoint(string host, int port, System.Net.Sockets.AddressFamily addressFamily)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException(SR.GetString("net_emptystringcall", new object[] { "host" }));
            }
            if ((port < 0) || (port > 0xffff))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if (((addressFamily != System.Net.Sockets.AddressFamily.InterNetwork) && (addressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)) && (addressFamily != System.Net.Sockets.AddressFamily.Unspecified))
            {
                throw new ArgumentException(SR.GetString("net_sockets_invalid_optionValue_all"), "addressFamily");
            }
            this.m_Host = host;
            this.m_Port = port;
            this.m_Family = addressFamily;
        }

        public override bool Equals(object comparand)
        {
            DnsEndPoint point = comparand as DnsEndPoint;
            if (point == null)
            {
                return false;
            }
            return (((this.m_Family == point.m_Family) && (this.m_Port == point.m_Port)) && (this.m_Host == point.m_Host));
        }

        public override int GetHashCode()
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.ToString());
        }

        public override string ToString()
        {
            return string.Concat(new object[] { this.m_Family, "/", this.m_Host, ":", this.m_Port });
        }

        public override System.Net.Sockets.AddressFamily AddressFamily
        {
            get
            {
                return this.m_Family;
            }
        }

        public string Host
        {
            get
            {
                return this.m_Host;
            }
        }

        public int Port
        {
            get
            {
                return this.m_Port;
            }
        }
    }
}

