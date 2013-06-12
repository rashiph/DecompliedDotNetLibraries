namespace System.Net.Sockets
{
    using System;
    using System.Net;

    public class IPv6MulticastOption
    {
        private IPAddress m_Group;
        private long m_Interface;

        public IPv6MulticastOption(IPAddress group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            this.Group = group;
            this.InterfaceIndex = 0L;
        }

        public IPv6MulticastOption(IPAddress group, long ifindex)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            if ((ifindex < 0L) || (ifindex > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("ifindex");
            }
            this.Group = group;
            this.InterfaceIndex = ifindex;
        }

        public IPAddress Group
        {
            get
            {
                return this.m_Group;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_Group = value;
            }
        }

        public long InterfaceIndex
        {
            get
            {
                return this.m_Interface;
            }
            set
            {
                if ((value < 0L) || (value > 0xffffffffL))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_Interface = value;
            }
        }
    }
}

