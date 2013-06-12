namespace System.Net
{
    using System;
    using System.Globalization;
    using System.Security;

    [Serializable]
    public class EndpointPermission
    {
        internal IPAddress[] address;
        internal bool cached;
        private static char[] DotSeparator = new char[] { '.' };
        private const string encSeperator = "#";
        internal string hostname;
        internal int port;
        internal TransportType transport;
        internal bool wildcard;

        internal EndpointPermission(string epname, int port, TransportType trtype)
        {
            if (CheckEndPointName(epname) == EndPointType.Invalid)
            {
                throw new ArgumentException(SR.GetString("net_perm_epname", new object[] { epname }), "epname");
            }
            if (!ValidationHelper.ValidateTcpPort(port) && (port != -1))
            {
                throw new ArgumentOutOfRangeException("port", SR.GetString("net_perm_invalid_val", new object[] { "Port", port.ToString(NumberFormatInfo.InvariantInfo) }));
            }
            this.hostname = epname;
            this.port = port;
            this.transport = trtype;
            this.wildcard = false;
        }

        private static EndPointType CheckEndPointName(string name)
        {
            if (name != null)
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                for (int i = 0; i < name.Length; i++)
                {
                    char ch = name[i];
                    char ch2 = ch;
                    if (ch2 <= '.')
                    {
                        switch (ch2)
                        {
                            case '*':
                            case '-':
                                goto Label_0057;

                            case '.':
                            {
                                continue;
                            }
                            case '%':
                                goto Label_005B;
                        }
                        goto Label_005F;
                    }
                    if (ch2 == ':')
                    {
                        goto Label_005B;
                    }
                    if (ch2 != '_')
                    {
                        goto Label_005F;
                    }
                Label_0057:
                    flag2 = true;
                    continue;
                Label_005B:
                    flag = true;
                    continue;
                Label_005F:
                    if (((ch > 'f') && (ch <= 'z')) || ((ch > 'F') && (ch <= 'Z')))
                    {
                        flag2 = true;
                    }
                    else if (((ch >= 'a') && (ch <= 'f')) || ((ch >= 'A') && (ch <= 'F')))
                    {
                        flag3 = true;
                    }
                    else if ((ch < '0') || (ch > '9'))
                    {
                        return EndPointType.Invalid;
                    }
                }
                if (!flag)
                {
                    if (!flag2 && !flag3)
                    {
                        return EndPointType.IPv4;
                    }
                    return EndPointType.DnsOrWildcard;
                }
                if (!flag2)
                {
                    return EndPointType.IPv6;
                }
            }
            return EndPointType.Invalid;
        }

        public override bool Equals(object obj)
        {
            EndpointPermission permission = (EndpointPermission) obj;
            if (string.Compare(this.hostname, permission.hostname, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }
            if (this.port != permission.port)
            {
                return false;
            }
            if (this.transport != permission.transport)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        internal EndpointPermission Intersect(EndpointPermission E)
        {
            string epname = null;
            TransportType transport;
            int port;
            if (this.transport == E.transport)
            {
                transport = this.transport;
            }
            else if (this.transport == TransportType.All)
            {
                transport = E.transport;
            }
            else if (E.transport == TransportType.All)
            {
                transport = this.transport;
            }
            else
            {
                return null;
            }
            if (this.port == E.port)
            {
                port = this.port;
            }
            else if (this.port == -1)
            {
                port = E.port;
            }
            else if (E.port == -1)
            {
                port = this.port;
            }
            else
            {
                return null;
            }
            if (this.Hostname.Equals("0.0.0.0"))
            {
                if (!E.Hostname.Equals("*.*.*.*") && !E.Hostname.Equals("0.0.0.0"))
                {
                    return null;
                }
                epname = this.Hostname;
            }
            else if (E.Hostname.Equals("0.0.0.0"))
            {
                if (!this.Hostname.Equals("*.*.*.*") && !this.Hostname.Equals("0.0.0.0"))
                {
                    return null;
                }
                epname = E.Hostname;
            }
            else if (this.IsDns && E.IsDns)
            {
                if (string.Compare(this.hostname, E.hostname, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return null;
                }
                epname = this.hostname;
            }
            else
            {
                this.Resolve();
                E.Resolve();
                if (((this.address == null) && !this.wildcard) || ((E.address == null) && !E.wildcard))
                {
                    return null;
                }
                if (this.wildcard && E.wildcard)
                {
                    string[] strArray = this.hostname.Split(DotSeparator);
                    string[] strArray2 = E.hostname.Split(DotSeparator);
                    string str2 = "";
                    if ((strArray2.Length != 4) || (strArray.Length != 4))
                    {
                        return null;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        if (i != 0)
                        {
                            str2 = str2 + ".";
                        }
                        if (strArray2[i] == strArray[i])
                        {
                            str2 = str2 + strArray2[i];
                        }
                        else if (strArray2[i] == "*")
                        {
                            str2 = str2 + strArray[i];
                        }
                        else if (strArray[i] == "*")
                        {
                            str2 = str2 + strArray2[i];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    epname = str2;
                }
                else if (this.wildcard)
                {
                    for (int j = 0; j < E.address.Length; j++)
                    {
                        if (this.MatchWildcard(E.address[j].ToString()))
                        {
                            epname = E.hostname;
                            break;
                        }
                    }
                }
                else if (E.wildcard)
                {
                    for (int k = 0; k < this.address.Length; k++)
                    {
                        if (E.MatchWildcard(this.address[k].ToString()))
                        {
                            epname = this.hostname;
                            break;
                        }
                    }
                }
                else
                {
                    if (this.address == E.address)
                    {
                        epname = this.hostname;
                    }
                    for (int m = 0; (epname == null) && (m < this.address.Length); m++)
                    {
                        for (int n = 0; n < E.address.Length; n++)
                        {
                            if (this.address[m].Equals(E.address[n]))
                            {
                                epname = this.hostname;
                                break;
                            }
                        }
                    }
                }
                if (epname == null)
                {
                    return null;
                }
            }
            return new EndpointPermission(epname, port, transport);
        }

        internal bool MatchAddress(EndpointPermission e)
        {
            if ((this.Hostname.Length != 0) && (e.Hostname.Length != 0))
            {
                if (this.Hostname.Equals("0.0.0.0"))
                {
                    if (!e.Hostname.Equals("*.*.*.*") && !e.Hostname.Equals("0.0.0.0"))
                    {
                        return false;
                    }
                    return true;
                }
                if (this.IsDns && e.IsDns)
                {
                    return (string.Compare(this.hostname, e.hostname, StringComparison.OrdinalIgnoreCase) == 0);
                }
                this.Resolve();
                e.Resolve();
                if (((this.address == null) && !this.wildcard) || ((e.address == null) && !e.wildcard))
                {
                    return false;
                }
                if (this.wildcard && !e.wildcard)
                {
                    return false;
                }
                if (e.wildcard)
                {
                    if (this.wildcard)
                    {
                        if (this.MatchWildcard(e.hostname))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.address.Length; i++)
                        {
                            if (e.MatchWildcard(this.address[i].ToString()))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < this.address.Length; j++)
                    {
                        for (int k = 0; k < e.address.Length; k++)
                        {
                            if (this.address[j].Equals(e.address[k]))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal bool MatchWildcard(string str)
        {
            string[] strArray = this.hostname.Split(DotSeparator);
            string[] strArray2 = str.Split(DotSeparator);
            if ((strArray2.Length != 4) || (strArray.Length != 4))
            {
                return false;
            }
            for (int i = 0; i < 4; i++)
            {
                if ((strArray2[i] != strArray[i]) && (strArray[i] != "*"))
                {
                    return false;
                }
            }
            return true;
        }

        internal void Resolve()
        {
            if (!this.cached && !this.wildcard)
            {
                if (this.IsValidWildcard)
                {
                    this.wildcard = true;
                    this.cached = true;
                }
                else
                {
                    IPAddress address;
                    if (IPAddress.TryParse(this.hostname, out address))
                    {
                        this.address = new IPAddress[] { address };
                        this.cached = true;
                    }
                    else
                    {
                        try
                        {
                            bool flag;
                            IPHostEntry entry = Dns.InternalResolveFast(this.hostname, -1, out flag);
                            if (entry != null)
                            {
                                this.address = entry.AddressList;
                            }
                        }
                        catch (SecurityException)
                        {
                            throw;
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        internal bool SubsetMatch(EndpointPermission e)
        {
            return ((((this.transport == e.transport) || (e.transport == TransportType.All)) && (((this.port == e.port) || (e.port == -1)) || (this.port == 0))) && this.MatchAddress(e));
        }

        public override string ToString()
        {
            return string.Concat(new object[] { this.hostname, "#", this.port, "#", ((int) this.transport).ToString(NumberFormatInfo.InvariantInfo) });
        }

        public string Hostname
        {
            get
            {
                return this.hostname;
            }
        }

        internal bool IsDns
        {
            get
            {
                if (this.IsValidWildcard)
                {
                    return false;
                }
                return (CheckEndPointName(this.hostname) == EndPointType.DnsOrWildcard);
            }
        }

        private bool IsValidWildcard
        {
            get
            {
                int length = this.hostname.Length;
                if (length < 3)
                {
                    return false;
                }
                if ((this.hostname[0] == '.') || (this.hostname[length - 1] == '.'))
                {
                    return false;
                }
                int num2 = 0;
                int num3 = 0;
                for (int i = 0; i < this.hostname.Length; i++)
                {
                    if (this.hostname[i] == '.')
                    {
                        num2++;
                    }
                    else if (this.hostname[i] == '*')
                    {
                        num3++;
                    }
                    else if (!char.IsDigit(this.hostname[i]))
                    {
                        return false;
                    }
                }
                return ((num2 == 3) && (num3 > 0));
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
        }

        public TransportType Transport
        {
            get
            {
                return this.transport;
            }
        }

        private enum EndPointType
        {
            Invalid,
            IPv6,
            DnsOrWildcard,
            IPv4
        }
    }
}

