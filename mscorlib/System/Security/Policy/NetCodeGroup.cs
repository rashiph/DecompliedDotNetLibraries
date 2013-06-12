namespace System.Security.Policy
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;

    [Serializable, ComVisible(true)]
    public sealed class NetCodeGroup : CodeGroup, IUnionSemanticCodeGroup
    {
        public static readonly string AbsentOriginScheme = string.Empty;
        public static readonly string AnyOtherOriginScheme = CodeConnectAccess.AnyScheme;
        private const string c_AnyScheme = @"([0-9a-z+\-\.]+)://";
        private const string c_IgnoreUserInfo = "";
        private static readonly char[] c_SomeRegexChars = new char[] { '.', '-', '+', '[', ']', '{', '$', '^', '#', ')', '(', ' ' };
        [OptionalField(VersionAdded=2)]
        private ArrayList m_accessList;
        [OptionalField(VersionAdded=2)]
        private ArrayList m_schemesList;

        internal NetCodeGroup()
        {
            this.SetDefaults();
        }

        public NetCodeGroup(IMembershipCondition membershipCondition) : base(membershipCondition, (PolicyStatement) null)
        {
            this.SetDefaults();
        }

        public void AddConnectAccess(string originScheme, CodeConnectAccess connectAccess)
        {
            if (originScheme == null)
            {
                throw new ArgumentNullException("originScheme");
            }
            if (((originScheme != AbsentOriginScheme) && (originScheme != AnyOtherOriginScheme)) && !CodeConnectAccess.IsValidScheme(originScheme))
            {
                throw new ArgumentOutOfRangeException("originScheme");
            }
            if ((originScheme == AbsentOriginScheme) && connectAccess.IsOriginScheme)
            {
                throw new ArgumentOutOfRangeException("connectAccess");
            }
            if (this.m_schemesList == null)
            {
                this.m_schemesList = new ArrayList();
                this.m_accessList = new ArrayList();
            }
            originScheme = originScheme.ToLower(CultureInfo.InvariantCulture);
            for (int i = 0; i < this.m_schemesList.Count; i++)
            {
                if (((string) this.m_schemesList[i]) == originScheme)
                {
                    if (connectAccess != null)
                    {
                        ArrayList list = (ArrayList) this.m_accessList[i];
                        for (i = 0; i < list.Count; i++)
                        {
                            if (((CodeConnectAccess) list[i]).Equals(connectAccess))
                            {
                                return;
                            }
                        }
                        list.Add(connectAccess);
                    }
                    return;
                }
            }
            this.m_schemesList.Add(originScheme);
            ArrayList list2 = new ArrayList();
            this.m_accessList.Add(list2);
            if (connectAccess != null)
            {
                list2.Add(connectAccess);
            }
        }

        private PolicyStatement CalculateAssemblyPolicy(Evidence evidence)
        {
            PolicyStatement statement = null;
            Url hostEvidence = evidence.GetHostEvidence<Url>();
            if (hostEvidence != null)
            {
                statement = this.CalculatePolicy(hostEvidence.GetURLString().Host, hostEvidence.GetURLString().Scheme, hostEvidence.GetURLString().Port);
            }
            else
            {
                Site site = evidence.GetHostEvidence<Site>();
                if (site != null)
                {
                    statement = this.CalculatePolicy(site.Name, null, null);
                }
            }
            if (statement == null)
            {
                statement = new PolicyStatement(new PermissionSet(false), PolicyStatementAttribute.Nothing);
            }
            return statement;
        }

        internal PolicyStatement CalculatePolicy(string host, string scheme, string port)
        {
            SecurityElement child = this.CreateWebPermission(host, scheme, port, null);
            SecurityElement et = new SecurityElement("PolicyStatement");
            SecurityElement element3 = new SecurityElement("PermissionSet");
            element3.AddAttribute("class", "System.Security.PermissionSet");
            element3.AddAttribute("version", "1");
            if (child != null)
            {
                element3.AddChild(child);
            }
            et.AddChild(element3);
            PolicyStatement statement = new PolicyStatement();
            statement.FromXml(et);
            return statement;
        }

        [SecuritySafeCritical]
        public override CodeGroup Copy()
        {
            NetCodeGroup group = new NetCodeGroup(base.MembershipCondition) {
                Name = base.Name,
                Description = base.Description
            };
            if (this.m_schemesList != null)
            {
                group.m_schemesList = (ArrayList) this.m_schemesList.Clone();
                group.m_accessList = new ArrayList(this.m_accessList.Count);
                for (int i = 0; i < this.m_accessList.Count; i++)
                {
                    group.m_accessList.Add(((ArrayList) this.m_accessList[i]).Clone());
                }
            }
            IEnumerator enumerator = base.Children.GetEnumerator();
            while (enumerator.MoveNext())
            {
                group.AddChild((CodeGroup) enumerator.Current);
            }
            return group;
        }

        internal SecurityElement CreateWebPermission(string host, string scheme, string port, string assemblyOverride)
        {
            if (scheme == null)
            {
                scheme = string.Empty;
            }
            if ((host == null) || (host.Length == 0))
            {
                return null;
            }
            host = host.ToLower(CultureInfo.InvariantCulture);
            scheme = scheme.ToLower(CultureInfo.InvariantCulture);
            int intPort = -1;
            if ((port != null) && (port.Length != 0))
            {
                intPort = int.Parse(port, CultureInfo.InvariantCulture);
            }
            else
            {
                port = string.Empty;
            }
            CodeConnectAccess[] access = this.FindAccessRulesForScheme(scheme);
            if ((access == null) || (access.Length == 0))
            {
                return null;
            }
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", "System.Net.WebPermission, " + ((assemblyOverride == null) ? "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" : assemblyOverride));
            element.AddAttribute("version", "1");
            SecurityElement child = new SecurityElement("ConnectAccess");
            host = this.EscapeStringForRegex(host);
            scheme = this.EscapeStringForRegex(scheme);
            string str2 = this.TryPermissionAsOneString(access, scheme, host, intPort);
            if (str2 != null)
            {
                SecurityElement element3 = new SecurityElement("URI");
                element3.AddAttribute("uri", str2);
                child.AddChild(element3);
            }
            else
            {
                if (port.Length != 0)
                {
                    port = ":" + port;
                }
                for (int i = 0; i < access.Length; i++)
                {
                    str2 = this.GetPermissionAccessElementString(access[i], scheme, host, port);
                    SecurityElement element4 = new SecurityElement("URI");
                    element4.AddAttribute("uri", str2);
                    child.AddChild(element4);
                }
            }
            element.AddChild(child);
            return element;
        }

        protected override void CreateXml(SecurityElement element, PolicyLevel level)
        {
            DictionaryEntry[] connectAccessRules = this.GetConnectAccessRules();
            if (connectAccessRules != null)
            {
                SecurityElement child = new SecurityElement("connectAccessRules");
                foreach (DictionaryEntry entry in connectAccessRules)
                {
                    SecurityElement element3 = new SecurityElement("codeOrigin");
                    element3.AddAttribute("scheme", (string) entry.Key);
                    foreach (CodeConnectAccess access in (CodeConnectAccess[]) entry.Value)
                    {
                        SecurityElement element4 = new SecurityElement("connectAccess");
                        element4.AddAttribute("scheme", access.Scheme);
                        element4.AddAttribute("port", access.StrPort);
                        element3.AddChild(element4);
                    }
                    child.AddChild(element3);
                }
                element.AddChild(child);
            }
        }

        [SecurityCritical, Conditional("_DEBUG")]
        private static void DEBUG_OUT(string str)
        {
        }

        [SecuritySafeCritical]
        public override bool Equals(object o)
        {
            if (this != o)
            {
                NetCodeGroup group = o as NetCodeGroup;
                if ((group == null) || !base.Equals(group))
                {
                    return false;
                }
                if ((this.m_schemesList == null) != (group.m_schemesList == null))
                {
                    return false;
                }
                if (this.m_schemesList != null)
                {
                    if (this.m_schemesList.Count != group.m_schemesList.Count)
                    {
                        return false;
                    }
                    for (int i = 0; i < this.m_schemesList.Count; i++)
                    {
                        int index = group.m_schemesList.IndexOf(this.m_schemesList[i]);
                        if (index == -1)
                        {
                            return false;
                        }
                        ArrayList list = (ArrayList) this.m_accessList[i];
                        ArrayList list2 = (ArrayList) group.m_accessList[index];
                        if (list.Count != list2.Count)
                        {
                            return false;
                        }
                        for (int j = 0; j < list.Count; j++)
                        {
                            if (!list2.Contains(list[j]))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private string EscapeStringForRegex(string str)
        {
            int num2;
            int startIndex = 0;
            StringBuilder builder = null;
            while ((startIndex < str.Length) && ((num2 = str.IndexOfAny(c_SomeRegexChars, startIndex)) != -1))
            {
                if (builder == null)
                {
                    builder = new StringBuilder(str.Length * 2);
                }
                builder.Append(str, startIndex, num2 - startIndex).Append('\\').Append(str[num2]);
                startIndex = num2 + 1;
            }
            if (builder == null)
            {
                return str;
            }
            if (startIndex < str.Length)
            {
                builder.Append(str, startIndex, str.Length - startIndex);
            }
            return builder.ToString();
        }

        private CodeConnectAccess[] FindAccessRulesForScheme(string lowerCaseScheme)
        {
            if (this.m_schemesList == null)
            {
                return null;
            }
            int index = this.m_schemesList.IndexOf(lowerCaseScheme);
            if ((index == -1) && ((lowerCaseScheme == AbsentOriginScheme) || ((index = this.m_schemesList.IndexOf(AnyOtherOriginScheme)) == -1)))
            {
                return null;
            }
            ArrayList list = (ArrayList) this.m_accessList[index];
            return (CodeConnectAccess[]) list.ToArray(typeof(CodeConnectAccess));
        }

        public DictionaryEntry[] GetConnectAccessRules()
        {
            if (this.m_schemesList == null)
            {
                return null;
            }
            DictionaryEntry[] entryArray = new DictionaryEntry[this.m_schemesList.Count];
            for (int i = 0; i < entryArray.Length; i++)
            {
                entryArray[i].Key = this.m_schemesList[i];
                entryArray[i].Value = ((ArrayList) this.m_accessList[i]).ToArray(typeof(CodeConnectAccess));
            }
            return entryArray;
        }

        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            return (base.GetHashCode() + this.GetRulesHashCode());
        }

        private string GetPermissionAccessElementString(CodeConnectAccess access, string escapedScheme, string escapedHost, string strPort)
        {
            StringBuilder builder = new StringBuilder(((@"([0-9a-z+\-\.]+)://".Length * 2) + "".Length) + escapedHost.Length);
            if (access.IsAnyScheme)
            {
                builder.Append(@"([0-9a-z+\-\.]+)://");
            }
            else if (access.IsOriginScheme)
            {
                builder.Append(escapedScheme).Append("://");
            }
            else
            {
                builder.Append(this.EscapeStringForRegex(access.Scheme)).Append("://");
            }
            builder.Append("").Append(escapedHost);
            if (!access.IsDefaultPort)
            {
                if (access.IsOriginPort)
                {
                    builder.Append(strPort);
                }
                else
                {
                    builder.Append(':').Append(access.StrPort);
                }
            }
            builder.Append("/.*");
            return builder.ToString();
        }

        private int GetRulesHashCode()
        {
            if (this.m_schemesList == null)
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < this.m_schemesList.Count; i++)
            {
                num += ((string) this.m_schemesList[i]).GetHashCode();
            }
            foreach (ArrayList list in this.m_accessList)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    num += ((CodeConnectAccess) list[j]).GetHashCode();
                }
            }
            return num;
        }

        internal override string GetTypeName()
        {
            return "System.Security.Policy.NetCodeGroup";
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this.m_schemesList = null;
            this.m_accessList = null;
        }

        protected override void ParseXml(SecurityElement e, PolicyLevel level)
        {
            this.ResetConnectAccess();
            SecurityElement element = e.SearchForChildByTag("connectAccessRules");
            if ((element == null) || (element.Children == null))
            {
                this.SetDefaults();
            }
            else
            {
                foreach (SecurityElement element2 in element.Children)
                {
                    if (element2.Tag.Equals("codeOrigin"))
                    {
                        string originScheme = element2.Attribute("scheme");
                        bool flag = false;
                        if (element2.Children != null)
                        {
                            foreach (SecurityElement element3 in element2.Children)
                            {
                                if (element3.Tag.Equals("connectAccess"))
                                {
                                    string allowScheme = element3.Attribute("scheme");
                                    string allowPort = element3.Attribute("port");
                                    this.AddConnectAccess(originScheme, new CodeConnectAccess(allowScheme, allowPort));
                                    flag = true;
                                }
                            }
                        }
                        if (!flag)
                        {
                            this.AddConnectAccess(originScheme, null);
                        }
                    }
                }
            }
        }

        public void ResetConnectAccess()
        {
            this.m_schemesList = null;
            this.m_accessList = null;
        }

        [SecuritySafeCritical]
        public override PolicyStatement Resolve(Evidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException("evidence");
            }
            object usedEvidence = null;
            if (!PolicyManager.CheckMembershipCondition(base.MembershipCondition, evidence, out usedEvidence))
            {
                return null;
            }
            PolicyStatement statement = this.CalculateAssemblyPolicy(evidence);
            IDelayEvaluatedEvidence dependentEvidence = usedEvidence as IDelayEvaluatedEvidence;
            if ((dependentEvidence != null) && !dependentEvidence.IsVerified)
            {
                statement.AddDependentEvidence(dependentEvidence);
            }
            bool flag2 = false;
            IEnumerator enumerator = base.Children.GetEnumerator();
            while (enumerator.MoveNext() && !flag2)
            {
                PolicyStatement childPolicy = PolicyManager.ResolveCodeGroup(enumerator.Current as CodeGroup, evidence);
                if (childPolicy != null)
                {
                    statement.InplaceUnion(childPolicy);
                    if ((childPolicy.Attributes & PolicyStatementAttribute.Exclusive) == PolicyStatementAttribute.Exclusive)
                    {
                        flag2 = true;
                    }
                }
            }
            return statement;
        }

        [SecuritySafeCritical]
        public override CodeGroup ResolveMatchingCodeGroups(Evidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException("evidence");
            }
            if (!base.MembershipCondition.Check(evidence))
            {
                return null;
            }
            CodeGroup group = this.Copy();
            group.Children = new ArrayList();
            IEnumerator enumerator = base.Children.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CodeGroup group2 = ((CodeGroup) enumerator.Current).ResolveMatchingCodeGroups(evidence);
                if (group2 != null)
                {
                    group.AddChild(group2);
                }
            }
            return group;
        }

        private void SetDefaults()
        {
            this.AddConnectAccess("file", null);
            this.AddConnectAccess("http", new CodeConnectAccess("http", CodeConnectAccess.OriginPort));
            this.AddConnectAccess("http", new CodeConnectAccess("https", CodeConnectAccess.OriginPort));
            this.AddConnectAccess("https", new CodeConnectAccess("https", CodeConnectAccess.OriginPort));
            this.AddConnectAccess(AbsentOriginScheme, CodeConnectAccess.CreateAnySchemeAccess(CodeConnectAccess.OriginPort));
            this.AddConnectAccess(AnyOtherOriginScheme, CodeConnectAccess.CreateOriginSchemeAccess(CodeConnectAccess.OriginPort));
        }

        PolicyStatement IUnionSemanticCodeGroup.InternalResolve(Evidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException("evidence");
            }
            if (base.MembershipCondition.Check(evidence))
            {
                return this.CalculateAssemblyPolicy(evidence);
            }
            return null;
        }

        private string TryPermissionAsOneString(CodeConnectAccess[] access, string escapedScheme, string escapedHost, int intPort)
        {
            bool flag = true;
            bool flag2 = true;
            bool flag3 = false;
            int port = -2;
            for (int i = 0; i < access.Length; i++)
            {
                flag &= access[i].IsDefaultPort || (access[i].IsOriginPort && (intPort == -1));
                flag2 &= access[i].IsOriginPort || (access[i].Port == intPort);
                if (access[i].Port >= 0)
                {
                    if (port == -2)
                    {
                        port = access[i].Port;
                    }
                    else if (access[i].Port != port)
                    {
                        port = -1;
                    }
                }
                else
                {
                    port = -1;
                }
                if (access[i].IsAnyScheme)
                {
                    flag3 = true;
                }
            }
            if ((!flag && !flag2) && (port == -1))
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(((@"([0-9a-z+\-\.]+)://".Length * access.Length) + ("".Length * 2)) + escapedHost.Length);
            if (flag3)
            {
                builder.Append(@"([0-9a-z+\-\.]+)://");
            }
            else
            {
                builder.Append('(');
                for (int j = 0; j < access.Length; j++)
                {
                    int index = 0;
                    while (index < j)
                    {
                        if (access[j].Scheme == access[index].Scheme)
                        {
                            break;
                        }
                        index++;
                    }
                    if (index == j)
                    {
                        if (j != 0)
                        {
                            builder.Append('|');
                        }
                        builder.Append(access[j].IsOriginScheme ? escapedScheme : this.EscapeStringForRegex(access[j].Scheme));
                    }
                }
                builder.Append(")://");
            }
            builder.Append("").Append(escapedHost);
            if (!flag)
            {
                if (flag2)
                {
                    builder.Append(':').Append(intPort);
                }
                else
                {
                    builder.Append(':').Append(port);
                }
            }
            builder.Append("/.*");
            return builder.ToString();
        }

        public override string AttributeString
        {
            [SecuritySafeCritical]
            get
            {
                return null;
            }
        }

        public override string MergeLogic
        {
            get
            {
                return Environment.GetResourceString("MergeLogic_Union");
            }
        }

        public override string PermissionSetName
        {
            [SecuritySafeCritical]
            get
            {
                return Environment.GetResourceString("NetCodeGroup_PermissionSet");
            }
        }
    }
}

