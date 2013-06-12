namespace System.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [Serializable, SecurityPermission(SecurityAction.InheritanceDemand, ControlEvidence=true, ControlPolicy=true)]
    public abstract class ResourcePermissionBase : CodeAccessPermission, IUnrestrictedPermission
    {
        public const string Any = "*";
        private static string computerName;
        private bool isUnrestricted;
        public const string Local = ".";
        private Type permissionAccessType;
        private Hashtable rootTable;
        private string[] tagNames;

        protected ResourcePermissionBase()
        {
            this.rootTable = CreateHashtable();
        }

        protected ResourcePermissionBase(PermissionState state)
        {
            this.rootTable = CreateHashtable();
            if (state == PermissionState.Unrestricted)
            {
                this.isUnrestricted = true;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(SR.GetString("InvalidPermissionState"), "state");
                }
                this.isUnrestricted = false;
            }
        }

        protected void AddPermissionAccess(ResourcePermissionBaseEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            if (entry.PermissionAccessPath.Length != this.TagNames.Length)
            {
                throw new InvalidOperationException(SR.GetString("PermissionNumberOfElements"));
            }
            Hashtable rootTable = this.rootTable;
            string[] permissionAccessPath = entry.PermissionAccessPath;
            for (int i = 0; i < (permissionAccessPath.Length - 1); i++)
            {
                if (rootTable.ContainsKey(permissionAccessPath[i]))
                {
                    rootTable = (Hashtable) rootTable[permissionAccessPath[i]];
                }
                else
                {
                    Hashtable hashtable2 = CreateHashtable();
                    rootTable[permissionAccessPath[i]] = hashtable2;
                    rootTable = hashtable2;
                }
            }
            if (rootTable.ContainsKey(permissionAccessPath[permissionAccessPath.Length - 1]))
            {
                throw new InvalidOperationException(SR.GetString("PermissionItemExists"));
            }
            rootTable[permissionAccessPath[permissionAccessPath.Length - 1]] = entry.PermissionAccess;
        }

        protected void Clear()
        {
            this.rootTable.Clear();
        }

        public override IPermission Copy()
        {
            ResourcePermissionBase base2 = this.CreateInstance();
            base2.tagNames = this.tagNames;
            base2.permissionAccessType = this.permissionAccessType;
            base2.isUnrestricted = this.isUnrestricted;
            base2.rootTable = this.CopyChildren(this.rootTable, 0);
            return base2;
        }

        private Hashtable CopyChildren(object currentContent, int tagIndex)
        {
            IDictionaryEnumerator enumerator = ((Hashtable) currentContent).GetEnumerator();
            Hashtable hashtable = CreateHashtable();
            while (enumerator.MoveNext())
            {
                if (tagIndex < (this.TagNames.Length - 1))
                {
                    hashtable[enumerator.Key] = this.CopyChildren(enumerator.Value, tagIndex + 1);
                }
                else
                {
                    hashtable[enumerator.Key] = enumerator.Value;
                }
            }
            return hashtable;
        }

        private static Hashtable CreateHashtable()
        {
            return new Hashtable(StringComparer.OrdinalIgnoreCase);
        }

        private ResourcePermissionBase CreateInstance()
        {
            new PermissionSet(PermissionState.Unrestricted).Assert();
            return (ResourcePermissionBase) Activator.CreateInstance(base.GetType(), BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null);
        }

        public override void FromXml(SecurityElement securityElement)
        {
            if (securityElement == null)
            {
                throw new ArgumentNullException("securityElement");
            }
            if (!securityElement.Tag.Equals("Permission") && !securityElement.Tag.Equals("IPermission"))
            {
                throw new ArgumentException(SR.GetString("Argument_NotAPermissionElement"));
            }
            string str = securityElement.Attribute("version");
            if ((str != null) && !str.Equals("1"))
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidXMLBadVersion"));
            }
            string strA = securityElement.Attribute("Unrestricted");
            if ((strA != null) && (string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.isUnrestricted = true;
            }
            else
            {
                this.isUnrestricted = false;
                this.rootTable = (Hashtable) this.ReadChildren(securityElement, 0);
            }
        }

        private ResourcePermissionBaseEntry[] GetChildrenAccess(object currentContent, int tagIndex)
        {
            IDictionaryEnumerator enumerator = ((Hashtable) currentContent).GetEnumerator();
            ArrayList list = new ArrayList();
            while (enumerator.MoveNext())
            {
                if (tagIndex < (this.TagNames.Length - 1))
                {
                    ResourcePermissionBaseEntry[] childrenAccess = this.GetChildrenAccess(enumerator.Value, tagIndex + 1);
                    for (int i = 0; i < childrenAccess.Length; i++)
                    {
                        childrenAccess[i].PermissionAccessPath[tagIndex] = (string) enumerator.Key;
                    }
                    list.AddRange(childrenAccess);
                }
                else
                {
                    ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry((int) enumerator.Value, new string[this.TagNames.Length]);
                    entry.PermissionAccessPath[tagIndex] = (string) enumerator.Key;
                    list.Add(entry);
                }
            }
            return (ResourcePermissionBaseEntry[]) list.ToArray(typeof(ResourcePermissionBaseEntry));
        }

        protected ResourcePermissionBaseEntry[] GetPermissionEntries()
        {
            return this.GetChildrenAccess(this.rootTable, 0);
        }

        private bool HasContent(object value)
        {
            if (value != null)
            {
                if (value is int)
                {
                    int num = (int) value;
                    return (num != 0);
                }
                IDictionaryEnumerator enumerator = ((Hashtable) value).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (this.HasContent(enumerator.Value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            if (target.GetType() != base.GetType())
            {
                throw new ArgumentException(SR.GetString("PermissionTypeMismatch"), "target");
            }
            ResourcePermissionBase base2 = (ResourcePermissionBase) target;
            if (this.IsUnrestricted())
            {
                return base2.Copy();
            }
            if (base2.IsUnrestricted())
            {
                return this.Copy();
            }
            ResourcePermissionBase base3 = null;
            Hashtable hashtable = (Hashtable) this.IntersectContents(this.rootTable, base2.rootTable);
            if (hashtable != null)
            {
                base3 = this.CreateInstance();
                base3.rootTable = hashtable;
            }
            return base3;
        }

        private object IntersectContents(object currentContent, object targetContent)
        {
            IDictionaryEnumerator enumerator;
            Hashtable hashtable2;
            if (currentContent is int)
            {
                int num = (int) currentContent;
                int num2 = (int) targetContent;
                return (num & num2);
            }
            Hashtable hashtable = CreateHashtable();
            object obj2 = ((Hashtable) currentContent)["."];
            object obj3 = ((Hashtable) currentContent)[this.ComputerName];
            if ((obj2 != null) || (obj3 != null))
            {
                object obj4 = ((Hashtable) targetContent)["."];
                object obj5 = ((Hashtable) targetContent)[this.ComputerName];
                if ((obj4 != null) || (obj5 != null))
                {
                    object obj6 = obj2;
                    if ((obj2 != null) && (obj3 != null))
                    {
                        obj6 = this.UnionOfContents(obj2, obj3);
                    }
                    else if (obj3 != null)
                    {
                        obj6 = obj3;
                    }
                    object obj7 = obj4;
                    if ((obj4 != null) && (obj5 != null))
                    {
                        obj7 = this.UnionOfContents(obj4, obj5);
                    }
                    else if (obj5 != null)
                    {
                        obj7 = obj5;
                    }
                    object obj8 = this.IntersectContents(obj6, obj7);
                    if (this.HasContent(obj8))
                    {
                        if ((obj3 != null) || (obj5 != null))
                        {
                            hashtable[this.ComputerName] = obj8;
                        }
                        else
                        {
                            hashtable["."] = obj8;
                        }
                    }
                }
            }
            if (((Hashtable) currentContent).Count < ((Hashtable) targetContent).Count)
            {
                enumerator = ((Hashtable) currentContent).GetEnumerator();
                hashtable2 = (Hashtable) targetContent;
            }
            else
            {
                enumerator = ((Hashtable) targetContent).GetEnumerator();
                hashtable2 = (Hashtable) currentContent;
            }
            while (enumerator.MoveNext())
            {
                string key = (string) enumerator.Key;
                if ((hashtable2.ContainsKey(key) && (key != ".")) && (key != this.ComputerName))
                {
                    object obj9 = enumerator.Value;
                    object obj10 = hashtable2[key];
                    object obj11 = this.IntersectContents(obj9, obj10);
                    if (this.HasContent(obj11))
                    {
                        hashtable[key] = obj11;
                    }
                }
            }
            if (hashtable.Count <= 0)
            {
                return null;
            }
            return hashtable;
        }

        private bool IsContentSubset(object currentContent, object targetContent)
        {
            if (currentContent is int)
            {
                int num = (int) currentContent;
                int num2 = (int) targetContent;
                if ((num & num2) != num)
                {
                    return false;
                }
                return true;
            }
            Hashtable hashtable = (Hashtable) currentContent;
            Hashtable hashtable2 = (Hashtable) targetContent;
            object obj2 = hashtable2["*"];
            if (obj2 != null)
            {
                foreach (DictionaryEntry entry in hashtable)
                {
                    if (!this.IsContentSubset(entry.Value, obj2))
                    {
                        return false;
                    }
                }
                return true;
            }
            foreach (DictionaryEntry entry2 in hashtable)
            {
                string key = (string) entry2.Key;
                if ((this.HasContent(entry2.Value) && (key != ".")) && (key != this.ComputerName))
                {
                    if (!hashtable2.ContainsKey(key))
                    {
                        return false;
                    }
                    if (!this.IsContentSubset(entry2.Value, hashtable2[key]))
                    {
                        return false;
                    }
                }
            }
            object obj3 = this.MergeContents(hashtable["."], hashtable[this.ComputerName]);
            if (obj3 != null)
            {
                object obj4 = this.MergeContents(hashtable2["."], hashtable2[this.ComputerName]);
                if (obj4 != null)
                {
                    return this.IsContentSubset(obj3, obj4);
                }
                if (!this.IsEmpty)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return this.IsEmpty;
            }
            if (target.GetType() != base.GetType())
            {
                return false;
            }
            ResourcePermissionBase base2 = (ResourcePermissionBase) target;
            if (base2.IsUnrestricted())
            {
                return true;
            }
            if (this.IsUnrestricted())
            {
                return false;
            }
            return this.IsContentSubset(this.rootTable, base2.rootTable);
        }

        public bool IsUnrestricted()
        {
            return this.isUnrestricted;
        }

        private object MergeContents(object content1, object content2)
        {
            if (content1 == null)
            {
                if (content2 == null)
                {
                    return null;
                }
                return content2;
            }
            if (content2 == null)
            {
                return content1;
            }
            return this.UnionOfContents(content1, content2);
        }

        private object ReadChildren(SecurityElement securityElement, int tagIndex)
        {
            Hashtable hashtable = CreateHashtable();
            if (securityElement.Children != null)
            {
                for (int i = 0; i < securityElement.Children.Count; i++)
                {
                    SecurityElement element = (SecurityElement) securityElement.Children[i];
                    if (element.Tag == this.TagNames[tagIndex])
                    {
                        string str = element.Attribute("name");
                        if (tagIndex < (this.TagNames.Length - 1))
                        {
                            hashtable[str] = this.ReadChildren(element, tagIndex + 1);
                        }
                        else
                        {
                            string str2 = element.Attribute("access");
                            int num2 = 0;
                            if (str2 != null)
                            {
                                num2 = (int) Enum.Parse(this.PermissionAccessType, str2);
                            }
                            hashtable[str] = num2;
                        }
                    }
                }
            }
            return hashtable;
        }

        protected void RemovePermissionAccess(ResourcePermissionBaseEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            if (entry.PermissionAccessPath.Length != this.TagNames.Length)
            {
                throw new InvalidOperationException(SR.GetString("PermissionNumberOfElements"));
            }
            Hashtable rootTable = this.rootTable;
            string[] permissionAccessPath = entry.PermissionAccessPath;
            for (int i = 0; i < permissionAccessPath.Length; i++)
            {
                if ((rootTable == null) || !rootTable.ContainsKey(permissionAccessPath[i]))
                {
                    throw new InvalidOperationException(SR.GetString("PermissionItemDoesntExist"));
                }
                Hashtable hashtable2 = rootTable;
                if (i < (permissionAccessPath.Length - 1))
                {
                    rootTable = (Hashtable) rootTable[permissionAccessPath[i]];
                    if (rootTable.Count == 1)
                    {
                        hashtable2.Remove(permissionAccessPath[i]);
                    }
                }
                else
                {
                    rootTable = null;
                    hashtable2.Remove(permissionAccessPath[i]);
                }
            }
        }

        public override SecurityElement ToXml()
        {
            SecurityElement currentElement = new SecurityElement("IPermission");
            Type type = base.GetType();
            currentElement.AddAttribute("class", type.FullName + ", " + type.Module.Assembly.FullName.Replace('"', '\''));
            currentElement.AddAttribute("version", "1");
            if (this.isUnrestricted)
            {
                currentElement.AddAttribute("Unrestricted", "true");
                return currentElement;
            }
            this.WriteChildren(currentElement, this.rootTable, 0);
            return currentElement;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            if (target.GetType() != base.GetType())
            {
                throw new ArgumentException(SR.GetString("PermissionTypeMismatch"), "target");
            }
            ResourcePermissionBase base2 = (ResourcePermissionBase) target;
            ResourcePermissionBase base3 = null;
            if (this.IsUnrestricted() || base2.IsUnrestricted())
            {
                base3 = this.CreateInstance();
                base3.isUnrestricted = true;
                return base3;
            }
            Hashtable hashtable = (Hashtable) this.UnionOfContents(this.rootTable, base2.rootTable);
            if (hashtable != null)
            {
                base3 = this.CreateInstance();
                base3.rootTable = hashtable;
            }
            return base3;
        }

        private object UnionOfContents(object currentContent, object targetContent)
        {
            if (currentContent is int)
            {
                int num = (int) currentContent;
                int num2 = (int) targetContent;
                return (num | num2);
            }
            Hashtable hashtable = CreateHashtable();
            IDictionaryEnumerator enumerator = ((Hashtable) currentContent).GetEnumerator();
            IDictionaryEnumerator enumerator2 = ((Hashtable) targetContent).GetEnumerator();
            while (enumerator.MoveNext())
            {
                hashtable[(string) enumerator.Key] = enumerator.Value;
            }
            while (enumerator2.MoveNext())
            {
                if (!hashtable.ContainsKey(enumerator2.Key))
                {
                    hashtable[enumerator2.Key] = enumerator2.Value;
                }
                else
                {
                    object obj2 = hashtable[enumerator2.Key];
                    object obj3 = enumerator2.Value;
                    hashtable[enumerator2.Key] = this.UnionOfContents(obj2, obj3);
                }
            }
            if (hashtable.Count <= 0)
            {
                return null;
            }
            return hashtable;
        }

        private void WriteChildren(SecurityElement currentElement, object currentContent, int tagIndex)
        {
            IDictionaryEnumerator enumerator = ((Hashtable) currentContent).GetEnumerator();
            while (enumerator.MoveNext())
            {
                SecurityElement child = new SecurityElement(this.TagNames[tagIndex]);
                currentElement.AddChild(child);
                child.AddAttribute("name", (string) enumerator.Key);
                if (tagIndex < (this.TagNames.Length - 1))
                {
                    this.WriteChildren(child, enumerator.Value, tagIndex + 1);
                }
                else
                {
                    string str = null;
                    int num = (int) enumerator.Value;
                    if ((this.PermissionAccessType != null) && (num != 0))
                    {
                        str = Enum.Format(this.PermissionAccessType, num, "g");
                        child.AddAttribute("access", str);
                    }
                }
            }
        }

        private string ComputerName
        {
            get
            {
                if (computerName == null)
                {
                    lock (typeof(ResourcePermissionBase))
                    {
                        if (computerName == null)
                        {
                            StringBuilder lpBuffer = new StringBuilder(0x100);
                            int capacity = lpBuffer.Capacity;
                            UnsafeNativeMethods.GetComputerName(lpBuffer, ref capacity);
                            computerName = lpBuffer.ToString();
                        }
                    }
                }
                return computerName;
            }
        }

        private bool IsEmpty
        {
            get
            {
                return (!this.isUnrestricted && (this.rootTable.Count == 0));
            }
        }

        protected Type PermissionAccessType
        {
            get
            {
                return this.permissionAccessType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!value.IsEnum)
                {
                    throw new ArgumentException(SR.GetString("PermissionBadParameterEnum"), "value");
                }
                this.permissionAccessType = value;
            }
        }

        protected string[] TagNames
        {
            get
            {
                return this.tagNames;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException(SR.GetString("PermissionInvalidLength", new object[] { "0" }), "value");
                }
                this.tagNames = value;
            }
        }

        [SuppressUnmanagedCodeSecurity]
        private static class UnsafeNativeMethods
        {
            [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
            internal static extern bool GetComputerName(StringBuilder lpBuffer, ref int nSize);
        }
    }
}

