namespace System.DirectoryServices.Protocols
{
    using System;

    public class LdapDirectoryIdentifier : DirectoryIdentifier
    {
        private bool connectionless;
        private bool fullyQualifiedDnsHostName;
        private int portNumber;
        private string[] servers;

        public LdapDirectoryIdentifier(string server) : this((server != null) ? new string[] { server } : null, false, false)
        {
        }

        public LdapDirectoryIdentifier(string server, int portNumber) : this((server != null) ? new string[] { server } : null, portNumber, false, false)
        {
        }

        public LdapDirectoryIdentifier(string server, bool fullyQualifiedDnsHostName, bool connectionless) : this((server != null) ? new string[] { server } : null, fullyQualifiedDnsHostName, connectionless)
        {
        }

        public LdapDirectoryIdentifier(string[] servers, bool fullyQualifiedDnsHostName, bool connectionless)
        {
            this.portNumber = 0x185;
            if (servers != null)
            {
                this.servers = new string[servers.Length];
                for (int i = 0; i < servers.Length; i++)
                {
                    if (servers[i] != null)
                    {
                        string str = servers[i].Trim();
                        if (str.Split(new char[] { ' ' }).Length > 1)
                        {
                            throw new ArgumentException(Res.GetString("WhiteSpaceServerName"));
                        }
                        this.servers[i] = str;
                    }
                }
            }
            this.fullyQualifiedDnsHostName = fullyQualifiedDnsHostName;
            this.connectionless = connectionless;
        }

        public LdapDirectoryIdentifier(string server, int portNumber, bool fullyQualifiedDnsHostName, bool connectionless) : this((server != null) ? new string[] { server } : null, portNumber, fullyQualifiedDnsHostName, connectionless)
        {
        }

        public LdapDirectoryIdentifier(string[] servers, int portNumber, bool fullyQualifiedDnsHostName, bool connectionless) : this(servers, fullyQualifiedDnsHostName, connectionless)
        {
            this.portNumber = portNumber;
        }

        public bool Connectionless
        {
            get
            {
                return this.connectionless;
            }
        }

        public bool FullyQualifiedDnsHostName
        {
            get
            {
                return this.fullyQualifiedDnsHostName;
            }
        }

        public int PortNumber
        {
            get
            {
                return this.portNumber;
            }
        }

        public string[] Servers
        {
            get
            {
                if (this.servers == null)
                {
                    return new string[0];
                }
                string[] strArray = new string[this.servers.Length];
                for (int i = 0; i < this.servers.Length; i++)
                {
                    if (this.servers[i] != null)
                    {
                        strArray[i] = string.Copy(this.servers[i]);
                    }
                    else
                    {
                        strArray[i] = null;
                    }
                }
                return strArray;
            }
        }
    }
}

