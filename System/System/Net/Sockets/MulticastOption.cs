namespace System.Net.Sockets
{
    using System;
    using System.Net;

    public class MulticastOption
    {
        private IPAddress group;
        private int ifIndex;
        private IPAddress localAddress;

        public MulticastOption(IPAddress group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            this.Group = group;
            this.LocalAddress = IPAddress.Any;
        }

        public MulticastOption(IPAddress group, int interfaceIndex)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            if ((interfaceIndex < 0) || (interfaceIndex > 0xffffff))
            {
                throw new ArgumentOutOfRangeException("interfaceIndex");
            }
            if (!ComNetOS.IsPostWin2K)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
            }
            this.Group = group;
            this.ifIndex = interfaceIndex;
        }

        public MulticastOption(IPAddress group, IPAddress mcint)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }
            if (mcint == null)
            {
                throw new ArgumentNullException("mcint");
            }
            this.Group = group;
            this.LocalAddress = mcint;
        }

        public IPAddress Group
        {
            get
            {
                return this.group;
            }
            set
            {
                this.group = value;
            }
        }

        public int InterfaceIndex
        {
            get
            {
                return this.ifIndex;
            }
            set
            {
                if ((value < 0) || (value > 0xffffff))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (!ComNetOS.IsPostWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
                }
                this.localAddress = null;
                this.ifIndex = value;
            }
        }

        public IPAddress LocalAddress
        {
            get
            {
                return this.localAddress;
            }
            set
            {
                this.ifIndex = 0;
                this.localAddress = value;
            }
        }
    }
}

