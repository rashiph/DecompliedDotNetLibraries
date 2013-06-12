namespace System.Net
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;

    [Serializable]
    public sealed class WebPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private ArrayList m_acceptList;
        private ArrayList m_connectList;
        private bool m_noRestriction;
        [OptionalField]
        private bool m_UnrestrictedAccept;
        [OptionalField]
        private bool m_UnrestrictedConnect;
        internal const string MatchAll = ".*";
        private static Regex s_MatchAllRegex;

        public WebPermission()
        {
            this.m_connectList = new ArrayList();
            this.m_acceptList = new ArrayList();
        }

        internal WebPermission(bool unrestricted)
        {
            this.m_connectList = new ArrayList();
            this.m_acceptList = new ArrayList();
            this.m_noRestriction = unrestricted;
        }

        internal WebPermission(NetworkAccess access)
        {
            this.m_connectList = new ArrayList();
            this.m_acceptList = new ArrayList();
            this.m_UnrestrictedConnect = (access & NetworkAccess.Connect) != 0;
            this.m_UnrestrictedAccept = (access & NetworkAccess.Accept) != 0;
        }

        public WebPermission(PermissionState state)
        {
            this.m_connectList = new ArrayList();
            this.m_acceptList = new ArrayList();
            this.m_noRestriction = state == PermissionState.Unrestricted;
        }

        public WebPermission(NetworkAccess access, string uriString)
        {
            this.m_connectList = new ArrayList();
            this.m_acceptList = new ArrayList();
            this.AddPermission(access, uriString);
        }

        public WebPermission(NetworkAccess access, Regex uriRegex)
        {
            this.m_connectList = new ArrayList();
            this.m_acceptList = new ArrayList();
            this.AddPermission(access, uriRegex);
        }

        internal WebPermission(NetworkAccess access, Uri uri)
        {
            this.m_connectList = new ArrayList();
            this.m_acceptList = new ArrayList();
            this.AddPermission(access, uri);
        }

        internal void AddAsPattern(NetworkAccess access, DelayedRegex uriRegexPattern)
        {
            ArrayList list = new ArrayList();
            if (((access & NetworkAccess.Connect) != 0) && !this.m_UnrestrictedConnect)
            {
                list.Add(this.m_connectList);
            }
            if (((access & NetworkAccess.Accept) != 0) && !this.m_UnrestrictedAccept)
            {
                list.Add(this.m_acceptList);
            }
            foreach (ArrayList list2 in list)
            {
                bool flag = false;
                foreach (object obj2 in list2)
                {
                    if ((obj2 is DelayedRegex) && (string.Compare(uriRegexPattern.ToString(), obj2.ToString(), StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    list2.Add(uriRegexPattern);
                }
            }
        }

        public void AddPermission(NetworkAccess access, string uriString)
        {
            if (uriString == null)
            {
                throw new ArgumentNullException("uriString");
            }
            if (!this.m_noRestriction)
            {
                Uri uri;
                if (Uri.TryCreate(uriString, UriKind.Absolute, out uri))
                {
                    this.AddPermission(access, uri);
                }
                else
                {
                    ArrayList list = new ArrayList();
                    if (((access & NetworkAccess.Connect) != 0) && !this.m_UnrestrictedConnect)
                    {
                        list.Add(this.m_connectList);
                    }
                    if (((access & NetworkAccess.Accept) != 0) && !this.m_UnrestrictedAccept)
                    {
                        list.Add(this.m_acceptList);
                    }
                    foreach (ArrayList list2 in list)
                    {
                        bool flag = false;
                        foreach (object obj2 in list2)
                        {
                            string strA = obj2 as string;
                            if ((strA != null) && (string.Compare(strA, uriString, StringComparison.OrdinalIgnoreCase) == 0))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            list2.Add(uriString);
                        }
                    }
                }
            }
        }

        public void AddPermission(NetworkAccess access, Regex uriRegex)
        {
            if (uriRegex == null)
            {
                throw new ArgumentNullException("uriRegex");
            }
            if (!this.m_noRestriction)
            {
                if (uriRegex.ToString() == ".*")
                {
                    if (!this.m_UnrestrictedConnect && ((access & NetworkAccess.Connect) != 0))
                    {
                        this.m_UnrestrictedConnect = true;
                        this.m_connectList.Clear();
                    }
                    if (!this.m_UnrestrictedAccept && ((access & NetworkAccess.Accept) != 0))
                    {
                        this.m_UnrestrictedAccept = true;
                        this.m_acceptList.Clear();
                    }
                }
                else
                {
                    this.AddAsPattern(access, new DelayedRegex(uriRegex));
                }
            }
        }

        internal void AddPermission(NetworkAccess access, Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (!this.m_noRestriction)
            {
                ArrayList list = new ArrayList();
                if (((access & NetworkAccess.Connect) != 0) && !this.m_UnrestrictedConnect)
                {
                    list.Add(this.m_connectList);
                }
                if (((access & NetworkAccess.Accept) != 0) && !this.m_UnrestrictedAccept)
                {
                    list.Add(this.m_acceptList);
                }
                foreach (ArrayList list2 in list)
                {
                    bool flag = false;
                    foreach (object obj2 in list2)
                    {
                        if ((obj2 is Uri) && uri.Equals(obj2))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        list2.Add(uri);
                    }
                }
            }
        }

        public override IPermission Copy()
        {
            if (this.m_noRestriction)
            {
                return new WebPermission(true);
            }
            return new WebPermission((this.m_UnrestrictedConnect ? NetworkAccess.Connect : ((NetworkAccess) 0)) | (this.m_UnrestrictedAccept ? NetworkAccess.Accept : ((NetworkAccess) 0))) { m_acceptList = (ArrayList) this.m_acceptList.Clone(), m_connectList = (ArrayList) this.m_connectList.Clone() };
        }

        public override void FromXml(SecurityElement securityElement)
        {
            string str3;
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
            string strA = securityElement.Attribute("Unrestricted");
            this.m_connectList = new ArrayList();
            this.m_acceptList = new ArrayList();
            this.m_UnrestrictedAccept = this.m_UnrestrictedConnect = false;
            if ((strA != null) && (string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.m_noRestriction = true;
                return;
            }
            this.m_noRestriction = false;
            SecurityElement element = securityElement.SearchForChildByTag("ConnectAccess");
            if (element != null)
            {
                foreach (SecurityElement element2 in element.Children)
                {
                    if (element2.Tag.Equals("URI"))
                    {
                        try
                        {
                            str3 = element2.Attribute("uri");
                        }
                        catch
                        {
                            str3 = null;
                        }
                        switch (str3)
                        {
                            case null:
                                throw new ArgumentException(SR.GetString("net_perm_invalid_val_in_element"), "ConnectAccess");

                            case ".*":
                                this.m_UnrestrictedConnect = true;
                                this.m_connectList = new ArrayList();
                                goto Label_0196;
                        }
                        this.AddAsPattern(NetworkAccess.Connect, new DelayedRegex(str3));
                    }
                }
            }
        Label_0196:
            element = securityElement.SearchForChildByTag("AcceptAccess");
            if (element != null)
            {
                foreach (SecurityElement element3 in element.Children)
                {
                    if (element3.Tag.Equals("URI"))
                    {
                        try
                        {
                            str3 = element3.Attribute("uri");
                        }
                        catch
                        {
                            str3 = null;
                        }
                        switch (str3)
                        {
                            case null:
                                throw new ArgumentException(SR.GetString("net_perm_invalid_val_in_element"), "AcceptAccess");

                            case ".*":
                                this.m_UnrestrictedAccept = true;
                                this.m_acceptList = new ArrayList();
                                return;
                        }
                        this.AddAsPattern(NetworkAccess.Accept, new DelayedRegex(str3));
                    }
                }
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            WebPermission permission = target as WebPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (this.m_noRestriction)
            {
                return permission.Copy();
            }
            if (permission.m_noRestriction)
            {
                return this.Copy();
            }
            WebPermission permission2 = new WebPermission();
            if (this.m_UnrestrictedConnect && permission.m_UnrestrictedConnect)
            {
                permission2.m_UnrestrictedConnect = true;
            }
            else if (this.m_UnrestrictedConnect || permission.m_UnrestrictedConnect)
            {
                permission2.m_connectList = (ArrayList) (this.m_UnrestrictedConnect ? permission : this).m_connectList.Clone();
            }
            else
            {
                intersectList(this.m_connectList, permission.m_connectList, permission2.m_connectList);
            }
            if (this.m_UnrestrictedAccept && permission.m_UnrestrictedAccept)
            {
                permission2.m_UnrestrictedAccept = true;
            }
            else if (this.m_UnrestrictedAccept || permission.m_UnrestrictedAccept)
            {
                permission2.m_acceptList = (ArrayList) (this.m_UnrestrictedAccept ? permission : this).m_acceptList.Clone();
            }
            else
            {
                intersectList(this.m_acceptList, permission.m_acceptList, permission2.m_acceptList);
            }
            if ((!permission2.m_UnrestrictedConnect && !permission2.m_UnrestrictedAccept) && ((permission2.m_connectList.Count == 0) && (permission2.m_acceptList.Count == 0)))
            {
                return null;
            }
            return permission2;
        }

        private static void intersectList(ArrayList A, ArrayList B, ArrayList result)
        {
            int num2;
            bool[] flagArray = new bool[A.Count];
            bool[] flagArray2 = new bool[B.Count];
            int index = 0;
            foreach (object obj2 in A)
            {
                num2 = 0;
                foreach (object obj3 in B)
                {
                    if (!flagArray2[num2] && (obj2.GetType() == obj3.GetType()))
                    {
                        if (obj2 is Uri)
                        {
                            if (!obj2.Equals(obj3))
                            {
                                goto Label_00B7;
                            }
                            result.Add(obj2);
                            flagArray[index] = flagArray2[num2] = true;
                            break;
                        }
                        if (string.Compare(obj2.ToString(), obj3.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            result.Add(obj2);
                            flagArray[index] = flagArray2[num2] = true;
                            break;
                        }
                    }
                Label_00B7:
                    num2++;
                }
                index++;
            }
            index = 0;
            foreach (object obj4 in A)
            {
                if (!flagArray[index])
                {
                    num2 = 0;
                    foreach (object obj5 in B)
                    {
                        if (!flagArray2[num2])
                        {
                            bool flag;
                            object obj6 = intersectPair(obj4, obj5, out flag);
                            if (obj6 != null)
                            {
                                bool flag2 = false;
                                foreach (object obj7 in result)
                                {
                                    if ((flag == (obj7 is Uri)) && (flag ? obj6.Equals(obj7) : (string.Compare(obj7.ToString(), obj6.ToString(), StringComparison.OrdinalIgnoreCase) == 0)))
                                    {
                                        flag2 = true;
                                        break;
                                    }
                                }
                                if (!flag2)
                                {
                                    result.Add(obj6);
                                }
                            }
                        }
                        num2++;
                    }
                }
                index++;
            }
        }

        private static object intersectPair(object L, object R, out bool isUri)
        {
            isUri = false;
            DelayedRegex regex = L as DelayedRegex;
            DelayedRegex regex2 = R as DelayedRegex;
            if ((regex != null) && (regex2 != null))
            {
                return new DelayedRegex("(?=(" + regex.ToString() + "))(" + regex2.ToString() + ")");
            }
            if ((regex != null) && (regex2 == null))
            {
                isUri = R is Uri;
                string input = isUri ? ((Uri) R).GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped) : R.ToString();
                Match match = regex.AsRegex.Match(input);
                if (((match != null) && (match.Index == 0)) && (match.Length == input.Length))
                {
                    return R;
                }
                return null;
            }
            if ((regex == null) && (regex2 != null))
            {
                isUri = L is Uri;
                string str2 = isUri ? ((Uri) L).GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped) : L.ToString();
                Match match2 = regex2.AsRegex.Match(str2);
                if (((match2 != null) && (match2.Index == 0)) && (match2.Length == str2.Length))
                {
                    return L;
                }
                return null;
            }
            isUri = L is Uri;
            if (isUri)
            {
                if (!L.Equals(R))
                {
                    return null;
                }
                return L;
            }
            if (string.Compare(L.ToString(), R.ToString(), StringComparison.OrdinalIgnoreCase) != 0)
            {
                return null;
            }
            return L;
        }

        private static bool isMatchedURI(object uriToCheck, ArrayList uriPatternList)
        {
            string strA = uriToCheck as string;
            foreach (object obj2 in uriPatternList)
            {
                DelayedRegex regex = obj2 as DelayedRegex;
                if (regex == null)
                {
                    if (uriToCheck.GetType() == obj2.GetType())
                    {
                        if ((strA != null) && (string.Compare(strA, (string) obj2, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            return true;
                        }
                        if ((strA == null) && uriToCheck.Equals(obj2))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    string input = (strA != null) ? strA : ((Uri) uriToCheck).GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped);
                    Match match = regex.AsRegex.Match(input);
                    if (((match != null) && (match.Index == 0)) && (match.Length == input.Length))
                    {
                        return true;
                    }
                    if (strA == null)
                    {
                        input = ((Uri) uriToCheck).GetComponents(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped);
                        match = regex.AsRegex.Match(input);
                        if (((match != null) && (match.Index == 0)) && (match.Length == input.Length))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool isSpecialSubsetCase(string regexToCheck, ArrayList permList)
        {
            foreach (object obj2 in permList)
            {
                DelayedRegex regex = obj2 as DelayedRegex;
                if (regex != null)
                {
                    if (string.Compare(regexToCheck, regex.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
                else
                {
                    Uri uri = obj2 as Uri;
                    if (uri != null)
                    {
                        if (string.Compare(regexToCheck, Regex.Escape(uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped)), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
                        }
                    }
                    else if (string.Compare(regexToCheck, Regex.Escape(obj2.ToString()), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return (((!this.m_noRestriction && !this.m_UnrestrictedConnect) && (!this.m_UnrestrictedAccept && (this.m_connectList.Count == 0))) && (this.m_acceptList.Count == 0));
            }
            WebPermission permission = target as WebPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (!permission.m_noRestriction)
            {
                if (this.m_noRestriction)
                {
                    return false;
                }
                if (!permission.m_UnrestrictedAccept)
                {
                    if (this.m_UnrestrictedAccept)
                    {
                        return false;
                    }
                    if (this.m_acceptList.Count != 0)
                    {
                        if (permission.m_acceptList.Count == 0)
                        {
                            return false;
                        }
                        foreach (object obj2 in this.m_acceptList)
                        {
                            if (obj2 is DelayedRegex)
                            {
                                if (!isSpecialSubsetCase(obj2.ToString(), permission.m_acceptList))
                                {
                                    throw new NotSupportedException(SR.GetString("net_perm_both_regex"));
                                }
                            }
                            else if (!isMatchedURI(obj2, permission.m_acceptList))
                            {
                                return false;
                            }
                        }
                    }
                }
                if (!permission.m_UnrestrictedConnect)
                {
                    if (this.m_UnrestrictedConnect)
                    {
                        return false;
                    }
                    if (this.m_connectList.Count != 0)
                    {
                        if (permission.m_connectList.Count == 0)
                        {
                            return false;
                        }
                        foreach (object obj3 in this.m_connectList)
                        {
                            if (obj3 is DelayedRegex)
                            {
                                if (!isSpecialSubsetCase(obj3.ToString(), permission.m_connectList))
                                {
                                    throw new NotSupportedException(SR.GetString("net_perm_both_regex"));
                                }
                            }
                            else if (!isMatchedURI(obj3, permission.m_connectList))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public bool IsUnrestricted()
        {
            return this.m_noRestriction;
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", base.GetType().FullName + ", " + base.GetType().Module.Assembly.FullName.Replace('"', '\''));
            element.AddAttribute("version", "1");
            if (!this.IsUnrestricted())
            {
                string str = null;
                if (this.m_UnrestrictedConnect || (this.m_connectList.Count > 0))
                {
                    SecurityElement child = new SecurityElement("ConnectAccess");
                    if (this.m_UnrestrictedConnect)
                    {
                        SecurityElement element3 = new SecurityElement("URI");
                        element3.AddAttribute("uri", SecurityElement.Escape(".*"));
                        child.AddChild(element3);
                    }
                    else
                    {
                        foreach (object obj2 in this.m_connectList)
                        {
                            Uri uri = obj2 as Uri;
                            if (uri != null)
                            {
                                str = Regex.Escape(uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped));
                            }
                            else
                            {
                                str = obj2.ToString();
                            }
                            if (obj2 is string)
                            {
                                str = Regex.Escape(str);
                            }
                            SecurityElement element4 = new SecurityElement("URI");
                            element4.AddAttribute("uri", SecurityElement.Escape(str));
                            child.AddChild(element4);
                        }
                    }
                    element.AddChild(child);
                }
                if (this.m_UnrestrictedAccept || (this.m_acceptList.Count > 0))
                {
                    SecurityElement element5 = new SecurityElement("AcceptAccess");
                    if (this.m_UnrestrictedAccept)
                    {
                        SecurityElement element6 = new SecurityElement("URI");
                        element6.AddAttribute("uri", SecurityElement.Escape(".*"));
                        element5.AddChild(element6);
                    }
                    else
                    {
                        foreach (object obj3 in this.m_acceptList)
                        {
                            Uri uri2 = obj3 as Uri;
                            if (uri2 != null)
                            {
                                str = Regex.Escape(uri2.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped));
                            }
                            else
                            {
                                str = obj3.ToString();
                            }
                            if (obj3 is string)
                            {
                                str = Regex.Escape(str);
                            }
                            SecurityElement element7 = new SecurityElement("URI");
                            element7.AddAttribute("uri", SecurityElement.Escape(str));
                            element5.AddChild(element7);
                        }
                    }
                    element.AddChild(element5);
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
            WebPermission permission = target as WebPermission;
            if (permission == null)
            {
                throw new ArgumentException(SR.GetString("net_perm_target"), "target");
            }
            if (this.m_noRestriction || permission.m_noRestriction)
            {
                return new WebPermission(true);
            }
            WebPermission permission2 = new WebPermission();
            if (this.m_UnrestrictedConnect || permission.m_UnrestrictedConnect)
            {
                permission2.m_UnrestrictedConnect = true;
            }
            else
            {
                permission2.m_connectList = (ArrayList) permission.m_connectList.Clone();
                for (int j = 0; j < this.m_connectList.Count; j++)
                {
                    DelayedRegex uriRegexPattern = this.m_connectList[j] as DelayedRegex;
                    if (uriRegexPattern == null)
                    {
                        if (this.m_connectList[j] is string)
                        {
                            permission2.AddPermission(NetworkAccess.Connect, (string) this.m_connectList[j]);
                        }
                        else
                        {
                            permission2.AddPermission(NetworkAccess.Connect, (Uri) this.m_connectList[j]);
                        }
                    }
                    else
                    {
                        permission2.AddAsPattern(NetworkAccess.Connect, uriRegexPattern);
                    }
                }
            }
            if (this.m_UnrestrictedAccept || permission.m_UnrestrictedAccept)
            {
                permission2.m_UnrestrictedAccept = true;
                return permission2;
            }
            permission2.m_acceptList = (ArrayList) permission.m_acceptList.Clone();
            for (int i = 0; i < this.m_acceptList.Count; i++)
            {
                DelayedRegex regex2 = this.m_acceptList[i] as DelayedRegex;
                if (regex2 == null)
                {
                    if (this.m_acceptList[i] is string)
                    {
                        permission2.AddPermission(NetworkAccess.Accept, (string) this.m_acceptList[i]);
                    }
                    else
                    {
                        permission2.AddPermission(NetworkAccess.Accept, (Uri) this.m_acceptList[i]);
                    }
                }
                else
                {
                    permission2.AddAsPattern(NetworkAccess.Accept, regex2);
                }
            }
            return permission2;
        }

        public IEnumerator AcceptList
        {
            get
            {
                if (this.m_UnrestrictedAccept)
                {
                    return new Regex[] { MatchAllRegex }.GetEnumerator();
                }
                ArrayList list = new ArrayList(this.m_acceptList.Count);
                for (int i = 0; i < this.m_acceptList.Count; i++)
                {
                    list.Add((this.m_acceptList[i] is DelayedRegex) ? ((object) ((DelayedRegex) this.m_acceptList[i]).AsRegex) : ((this.m_acceptList[i] is Uri) ? ((object) ((Uri) this.m_acceptList[i]).GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped)) : this.m_acceptList[i]));
                }
                return list.GetEnumerator();
            }
        }

        public IEnumerator ConnectList
        {
            get
            {
                if (this.m_UnrestrictedConnect)
                {
                    return new Regex[] { MatchAllRegex }.GetEnumerator();
                }
                ArrayList list = new ArrayList(this.m_connectList.Count);
                for (int i = 0; i < this.m_connectList.Count; i++)
                {
                    list.Add((this.m_connectList[i] is DelayedRegex) ? ((object) ((DelayedRegex) this.m_connectList[i]).AsRegex) : ((this.m_connectList[i] is Uri) ? ((object) ((Uri) this.m_connectList[i]).GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped)) : this.m_connectList[i]));
                }
                return list.GetEnumerator();
            }
        }

        internal static Regex MatchAllRegex
        {
            get
            {
                if (s_MatchAllRegex == null)
                {
                    s_MatchAllRegex = new Regex(".*");
                }
                return s_MatchAllRegex;
            }
        }
    }
}

