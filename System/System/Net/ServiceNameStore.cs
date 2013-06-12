namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;

    internal class ServiceNameStore
    {
        private ServiceNameCollection serviceNameCollection = null;
        private List<string> serviceNames = new List<string>();

        public bool Add(string uriPrefix)
        {
            string[] strArray = this.BuildServiceNames(uriPrefix);
            bool flag = false;
            foreach (string str in strArray)
            {
                if (this.AddSingleServiceName(str))
                {
                    flag = true;
                    if (Logging.On)
                    {
                        Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" + ValidationHelper.HashString(this) + "::Add() adding default SPNs '" + str + "' from prefix '" + uriPrefix + "'");
                    }
                }
            }
            if (flag)
            {
                this.serviceNameCollection = null;
                return flag;
            }
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" + ValidationHelper.HashString(this) + "::Add() no default SPN added for prefix '" + uriPrefix + "'");
            }
            return flag;
        }

        private bool AddSingleServiceName(string spn)
        {
            if (this.Contains(spn))
            {
                return false;
            }
            this.serviceNames.Add(spn);
            return true;
        }

        public string[] BuildServiceNames(string uriPrefix)
        {
            string strA = this.ExtractHostname(uriPrefix, true);
            IPAddress address = null;
            if (((string.Compare(strA, "*", StringComparison.InvariantCultureIgnoreCase) == 0) || (string.Compare(strA, "+", StringComparison.InvariantCultureIgnoreCase) == 0)) || IPAddress.TryParse(strA, out address))
            {
                try
                {
                    string hostName = Dns.GetHostEntry(string.Empty).HostName;
                    return new string[] { ("HTTP/" + hostName) };
                }
                catch (SocketException)
                {
                    return new string[0];
                }
                catch (SecurityException)
                {
                    return new string[0];
                }
            }
            if (!strA.Contains("."))
            {
                try
                {
                    string str3 = Dns.GetHostEntry(strA).HostName;
                    return new string[] { ("HTTP/" + strA), ("HTTP/" + str3) };
                }
                catch (SocketException)
                {
                    return new string[] { ("HTTP/" + strA) };
                }
                catch (SecurityException)
                {
                    return new string[] { ("HTTP/" + strA) };
                }
            }
            return new string[] { ("HTTP/" + strA) };
        }

        public string BuildSimpleServiceName(string uriPrefix)
        {
            string str = this.ExtractHostname(uriPrefix, false);
            if (str != null)
            {
                return ("HTTP/" + str);
            }
            return null;
        }

        public void Clear()
        {
            this.serviceNames.Clear();
            this.serviceNameCollection = null;
        }

        private bool Contains(string newServiceName)
        {
            if (newServiceName == null)
            {
                return false;
            }
            foreach (string str in this.serviceNames)
            {
                if (string.Compare(str, newServiceName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private string ExtractHostname(string uriPrefix, bool allowInvalidUriStrings)
        {
            if (Uri.IsWellFormedUriString(uriPrefix, UriKind.Absolute))
            {
                Uri uri = new Uri(uriPrefix);
                return uri.Host;
            }
            if (!allowInvalidUriStrings)
            {
                return null;
            }
            int startIndex = uriPrefix.IndexOf("://") + 3;
            int num2 = startIndex;
            bool flag = false;
            while (((num2 < uriPrefix.Length) && (uriPrefix[num2] != '/')) && ((uriPrefix[num2] != ':') || flag))
            {
                if (uriPrefix[num2] == '[')
                {
                    if (flag)
                    {
                        num2 = startIndex;
                        break;
                    }
                    flag = true;
                }
                if (flag && (uriPrefix[num2] == ']'))
                {
                    flag = false;
                }
                num2++;
            }
            return uriPrefix.Substring(startIndex, num2 - startIndex);
        }

        public bool Remove(string uriPrefix)
        {
            string newServiceName = this.BuildSimpleServiceName(uriPrefix);
            bool flag = this.Contains(newServiceName);
            if (flag)
            {
                this.serviceNames.Remove(newServiceName);
                this.serviceNameCollection = null;
            }
            if (Logging.On)
            {
                if (flag)
                {
                    Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" + ValidationHelper.HashString(this) + "::Remove() removing default SPN '" + newServiceName + "' from prefix '" + uriPrefix + "'");
                    return flag;
                }
                Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" + ValidationHelper.HashString(this) + "::Remove() no default SPN removed for prefix '" + uriPrefix + "'");
            }
            return flag;
        }

        public ServiceNameCollection ServiceNames
        {
            get
            {
                if (this.serviceNameCollection == null)
                {
                    this.serviceNameCollection = new ServiceNameCollection(this.serviceNames);
                }
                return this.serviceNameCollection;
            }
        }
    }
}

