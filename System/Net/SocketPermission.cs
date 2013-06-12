namespace System.Net
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable]
    public sealed class SocketPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        public const int AllPorts = -1;
        internal const int AnyPort = 0;
        private ArrayList m_acceptList;
        private ArrayList m_connectList;
        private bool m_noRestriction;

        internal SocketPermission(bool free)
        {
            this.initialize();
            this.m_noRestriction = free;
        }

        public SocketPermission(PermissionState state)
        {
            this.initialize();
            this.m_noRestriction = state == PermissionState.Unrestricted;
        }

        public SocketPermission(NetworkAccess access, TransportType transport, string hostName, int portNumber)
        {
            this.initialize();
            this.m_noRestriction = false;
            this.AddPermission(access, transport, hostName, portNumber);
        }

        internal void AddPermission(NetworkAccess access, EndpointPermission endPoint)
        {
            if (!this.m_noRestriction)
            {
                if ((access & NetworkAccess.Connect) != 0)
                {
                    this.m_connectList.Add(endPoint);
                }
                if ((access & NetworkAccess.Accept) != 0)
                {
                    this.m_acceptList.Add(endPoint);
                }
            }
        }

        public void AddPermission(NetworkAccess access, TransportType transport, string hostName, int portNumber)
        {
            if (hostName == null)
            {
                throw new ArgumentNullException("hostName");
            }
            EndpointPermission endPoint = new EndpointPermission(hostName, portNumber, transport);
            this.AddPermission(access, endPoint);
        }

        private void CleanupDNS()
        {
            foreach (EndpointPermission permission in this.m_connectList)
            {
                if (!permission.cached)
                {
                    permission.address = null;
                }
            }
            foreach (EndpointPermission permission2 in this.m_acceptList)
            {
                if (!permission2.cached)
                {
                    permission2.address = null;
                }
            }
        }

        public override IPermission Copy()
        {
            return new SocketPermission(this.m_noRestriction) { m_connectList = (ArrayList) this.m_connectList.Clone(), m_acceptList = (ArrayList) this.m_acceptList.Clone() };
        }

        private bool FindSubset(ArrayList source, ArrayList target)
        {
            foreach (EndpointPermission permission in source)
            {
                bool flag = false;
                foreach (EndpointPermission permission2 in target)
                {
                    if (permission.SubsetMatch(permission2))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    return false;
                }
            }
            return true;
        }

        public override void FromXml(SecurityElement securityElement)
        {
            if (securityElement == null)
            {
                throw new ArgumentNullException("securityElement");
            }
            if (!securityElement.Tag.Equals("IPermission"))
            {
                throw new ArgumentException(SR.GetString("net_not_ipermission"), "securityElement");
            }
            string str = securityElement.Attribute("class");
            if (str == null)
            {
                throw new ArgumentException(SR.GetString("net_no_classname"), "securityElement");
            }
            if (str.IndexOf(base.GetType().FullName) < 0)
            {
                throw new ArgumentException(SR.GetString("net_no_typename"), "securityElement");
            }
            this.initialize();
            string strA = securityElement.Attribute("Unrestricted");
            if (strA != null)
            {
                this.m_noRestriction = 0 == string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase);
                if (this.m_noRestriction)
                {
                    return;
                }
            }
            this.m_noRestriction = false;
            this.m_connectList = new ArrayList();
            this.m_acceptList = new ArrayList();
            SecurityElement et = securityElement.SearchForChildByTag("ConnectAccess");
            if (et != null)
            {
                ParseAddXmlElement(et, this.m_connectList, "ConnectAccess, ");
            }
            et = securityElement.SearchForChildByTag("AcceptAccess");
            if (et != null)
            {
                ParseAddXmlElement(et, this.m_acceptList, "AcceptAccess, ");
            }
        }

        private void initialize()
        {
            this.m_noRestriction = false;
            this.m_connectList = new ArrayList();
            this.m_acceptList = new ArrayList();
        }

        public override IPermission Intersect(IPermission target)
        {
            SocketPermission permission2;
            if (target == null)
            {
                return null;
            }
            SocketPermission permission = target as SocketPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (this.m_noRestriction)
            {
                permission2 = (SocketPermission) permission.Copy();
            }
            else if (permission.m_noRestriction)
            {
                permission2 = (SocketPermission) this.Copy();
            }
            else
            {
                permission2 = new SocketPermission(false);
                intersectLists(this.m_connectList, permission.m_connectList, permission2.m_connectList);
                intersectLists(this.m_acceptList, permission.m_acceptList, permission2.m_acceptList);
            }
            if ((!permission2.m_noRestriction && (permission2.m_connectList.Count == 0)) && (permission2.m_acceptList.Count == 0))
            {
                return null;
            }
            return permission2;
        }

        private static void intersectLists(ArrayList A, ArrayList B, ArrayList result)
        {
            bool[] flagArray = new bool[A.Count];
            bool[] flagArray2 = new bool[B.Count];
            int index = 0;
            int num2 = 0;
            foreach (EndpointPermission permission in A)
            {
                num2 = 0;
                foreach (EndpointPermission permission2 in B)
                {
                    if (!flagArray2[num2] && permission.Equals(permission2))
                    {
                        result.Add(permission);
                        flagArray[index] = flagArray2[num2] = true;
                        break;
                    }
                    num2++;
                }
                index++;
            }
            index = 0;
            foreach (EndpointPermission permission3 in A)
            {
                if (!flagArray[index])
                {
                    num2 = 0;
                    foreach (EndpointPermission permission4 in B)
                    {
                        if (!flagArray2[num2])
                        {
                            EndpointPermission permission5 = permission3.Intersect(permission4);
                            if (permission5 != null)
                            {
                                bool flag = false;
                                foreach (EndpointPermission permission6 in result)
                                {
                                    if (permission6.Equals(permission5))
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    result.Add(permission5);
                                }
                            }
                        }
                        num2++;
                    }
                }
                index++;
            }
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return ((!this.m_noRestriction && (this.m_connectList.Count == 0)) && (this.m_acceptList.Count == 0));
            }
            SocketPermission permission = target as SocketPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (permission.IsUnrestricted())
            {
                return true;
            }
            if (this.IsUnrestricted())
            {
                return false;
            }
            if ((this.m_acceptList.Count + this.m_connectList.Count) == 0)
            {
                return true;
            }
            if ((permission.m_acceptList.Count + permission.m_connectList.Count) == 0)
            {
                return false;
            }
            bool flag = false;
            try
            {
                if (this.FindSubset(this.m_connectList, permission.m_connectList) && this.FindSubset(this.m_acceptList, permission.m_acceptList))
                {
                    flag = true;
                }
            }
            finally
            {
                this.CleanupDNS();
            }
            return flag;
        }

        public bool IsUnrestricted()
        {
            return this.m_noRestriction;
        }

        private static void ParseAddXmlElement(SecurityElement et, ArrayList listToAdd, string accessStr)
        {
            foreach (SecurityElement element in et.Children)
            {
                if (element.Tag.Equals("ENDPOINT"))
                {
                    string str;
                    TransportType type;
                    int num;
                    Hashtable attributes = element.Attributes;
                    try
                    {
                        str = attributes["host"] as string;
                    }
                    catch
                    {
                        str = null;
                    }
                    if (str == null)
                    {
                        throw new ArgumentNullException(accessStr + "host");
                    }
                    string epname = str;
                    try
                    {
                        str = attributes["transport"] as string;
                    }
                    catch
                    {
                        str = null;
                    }
                    if (str == null)
                    {
                        throw new ArgumentNullException(accessStr + "transport");
                    }
                    try
                    {
                        type = (TransportType) System.Enum.Parse(typeof(TransportType), str, true);
                    }
                    catch (Exception exception)
                    {
                        if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                        {
                            throw;
                        }
                        throw new ArgumentException(accessStr + "transport", exception);
                    }
                    try
                    {
                        str = attributes["port"] as string;
                    }
                    catch
                    {
                        str = null;
                    }
                    if (str == null)
                    {
                        throw new ArgumentNullException(accessStr + "port");
                    }
                    if (string.Compare(str, "All", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        str = "-1";
                    }
                    try
                    {
                        num = int.Parse(str, NumberFormatInfo.InvariantInfo);
                    }
                    catch (Exception exception2)
                    {
                        if (((exception2 is ThreadAbortException) || (exception2 is StackOverflowException)) || (exception2 is OutOfMemoryException))
                        {
                            throw;
                        }
                        throw new ArgumentException(SR.GetString("net_perm_invalid_val", new object[] { accessStr + "port", str }), exception2);
                    }
                    if (!ValidationHelper.ValidateTcpPort(num) && (num != -1))
                    {
                        throw new ArgumentOutOfRangeException("port", num, SR.GetString("net_perm_invalid_val", new object[] { accessStr + "port", str }));
                    }
                    listToAdd.Add(new EndpointPermission(epname, num, type));
                }
            }
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", base.GetType().FullName + ", " + base.GetType().Module.Assembly.FullName.Replace('"', '\''));
            element.AddAttribute("version", "1");
            if (!this.IsUnrestricted())
            {
                if (this.m_connectList.Count > 0)
                {
                    SecurityElement child = new SecurityElement("ConnectAccess");
                    foreach (EndpointPermission permission in this.m_connectList)
                    {
                        SecurityElement element3 = new SecurityElement("ENDPOINT");
                        element3.AddAttribute("host", permission.Hostname);
                        element3.AddAttribute("transport", permission.Transport.ToString());
                        element3.AddAttribute("port", (permission.Port != -1) ? permission.Port.ToString(NumberFormatInfo.InvariantInfo) : "All");
                        child.AddChild(element3);
                    }
                    element.AddChild(child);
                }
                if (this.m_acceptList.Count > 0)
                {
                    SecurityElement element4 = new SecurityElement("AcceptAccess");
                    foreach (EndpointPermission permission2 in this.m_acceptList)
                    {
                        SecurityElement element5 = new SecurityElement("ENDPOINT");
                        element5.AddAttribute("host", permission2.Hostname);
                        element5.AddAttribute("transport", permission2.Transport.ToString());
                        element5.AddAttribute("port", (permission2.Port != -1) ? permission2.Port.ToString(NumberFormatInfo.InvariantInfo) : "All");
                        element4.AddChild(element5);
                    }
                    element.AddChild(element4);
                }
                return element;
            }
            element.AddAttribute("Unrestricted", "true");
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            SocketPermission permission = target as SocketPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (this.m_noRestriction || permission.m_noRestriction)
            {
                return new SocketPermission(true);
            }
            SocketPermission permission2 = (SocketPermission) permission.Copy();
            for (int i = 0; i < this.m_connectList.Count; i++)
            {
                permission2.AddPermission(NetworkAccess.Connect, (EndpointPermission) this.m_connectList[i]);
            }
            for (int j = 0; j < this.m_acceptList.Count; j++)
            {
                permission2.AddPermission(NetworkAccess.Accept, (EndpointPermission) this.m_acceptList[j]);
            }
            return permission2;
        }

        public IEnumerator AcceptList
        {
            get
            {
                return this.m_acceptList.GetEnumerator();
            }
        }

        public IEnumerator ConnectList
        {
            get
            {
                return this.m_connectList.GetEnumerator();
            }
        }
    }
}

