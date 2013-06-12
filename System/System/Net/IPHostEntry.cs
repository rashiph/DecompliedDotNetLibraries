namespace System.Net
{
    using System;

    public class IPHostEntry
    {
        private IPAddress[] addressList;
        private string[] aliases;
        private string hostName;

        public IPAddress[] AddressList
        {
            get
            {
                return this.addressList;
            }
            set
            {
                this.addressList = value;
            }
        }

        public string[] Aliases
        {
            get
            {
                return this.aliases;
            }
            set
            {
                this.aliases = value;
            }
        }

        public string HostName
        {
            get
            {
                return this.hostName;
            }
            set
            {
                this.hostName = value;
            }
        }
    }
}

