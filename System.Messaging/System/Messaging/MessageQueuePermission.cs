namespace System.Messaging
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public sealed class MessageQueuePermission : CodeAccessPermission, IUnrestrictedPermission
    {
        internal const string Any = "*";
        internal MessageQueuePermissionEntryCollection innerCollection;
        internal bool isUnrestricted;
        internal Hashtable resolvedFormatNames;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MessageQueuePermission()
        {
        }

        public MessageQueuePermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.isUnrestricted = true;
            }
            else
            {
                this.isUnrestricted = false;
            }
        }

        public MessageQueuePermission(MessageQueuePermissionEntry[] permissionAccessEntries)
        {
            if (permissionAccessEntries == null)
            {
                throw new ArgumentNullException("permissionAccessEntries");
            }
            this.PermissionEntries.AddRange(permissionAccessEntries);
        }

        public MessageQueuePermission(MessageQueuePermissionAccess permissionAccess, string path)
        {
            MessageQueuePermissionEntry entry = new MessageQueuePermissionEntry(permissionAccess, path);
            this.PermissionEntries.Add(entry);
        }

        public MessageQueuePermission(MessageQueuePermissionAccess permissionAccess, string machineName, string label, string category)
        {
            MessageQueuePermissionEntry entry = new MessageQueuePermissionEntry(permissionAccess, machineName, label, category);
            this.PermissionEntries.Add(entry);
        }

        internal void Clear()
        {
            this.resolvedFormatNames = null;
        }

        public override IPermission Copy()
        {
            MessageQueuePermission permission = new MessageQueuePermission {
                isUnrestricted = this.isUnrestricted
            };
            foreach (MessageQueuePermissionEntry entry in this.PermissionEntries)
            {
                permission.PermissionEntries.Add(entry);
            }
            permission.resolvedFormatNames = this.resolvedFormatNames;
            return permission;
        }

        public override void FromXml(SecurityElement securityElement)
        {
            this.PermissionEntries.Clear();
            string strA = securityElement.Attribute("Unrestricted");
            if ((strA != null) && (string.Compare(strA, "true", true, CultureInfo.InvariantCulture) == 0))
            {
                this.isUnrestricted = true;
            }
            else if (securityElement.Children != null)
            {
                for (int i = 0; i < securityElement.Children.Count; i++)
                {
                    SecurityElement element = (SecurityElement) securityElement.Children[i];
                    MessageQueuePermissionEntry entry = null;
                    string str2 = element.Attribute("access");
                    int num2 = 0;
                    if (str2 != null)
                    {
                        string[] strArray = str2.Split(new char[] { '|' });
                        for (int j = 0; j < strArray.Length; j++)
                        {
                            string str3 = strArray[j].Trim();
                            if (Enum.IsDefined(typeof(MessageQueuePermissionAccess), str3))
                            {
                                num2 |= (int) Enum.Parse(typeof(MessageQueuePermissionAccess), str3);
                            }
                        }
                    }
                    if (element.Tag == "Path")
                    {
                        string path = element.Attribute("value");
                        if (path == null)
                        {
                            throw new InvalidOperationException(Res.GetString("InvalidXmlFormat"));
                        }
                        entry = new MessageQueuePermissionEntry((MessageQueuePermissionAccess) num2, path);
                    }
                    else
                    {
                        if (!(element.Tag == "Criteria"))
                        {
                            throw new InvalidOperationException(Res.GetString("InvalidXmlFormat"));
                        }
                        string label = element.Attribute("label");
                        string category = element.Attribute("category");
                        string machineName = element.Attribute("machine");
                        if (((machineName == null) && (label == null)) && (category == null))
                        {
                            throw new InvalidOperationException(Res.GetString("InvalidXmlFormat"));
                        }
                        entry = new MessageQueuePermissionEntry((MessageQueuePermissionAccess) num2, machineName, label, category);
                    }
                    this.PermissionEntries.Add(entry);
                }
            }
        }

        private static IEqualityComparer GetComparer()
        {
            return StringComparer.InvariantCultureIgnoreCase;
        }

        public override IPermission Intersect(IPermission target)
        {
            IDictionaryEnumerator enumerator;
            Hashtable resolvedFormatNames;
            if (target == null)
            {
                return null;
            }
            if (!(target is MessageQueuePermission))
            {
                throw new ArgumentException(Res.GetString("InvalidParameter", new object[] { "target", target.ToString() }));
            }
            MessageQueuePermission permission = (MessageQueuePermission) target;
            if (this.IsUnrestricted())
            {
                return permission.Copy();
            }
            if (permission.IsUnrestricted())
            {
                return this.Copy();
            }
            this.ResolveFormatNames();
            permission.ResolveFormatNames();
            MessageQueuePermission permission2 = new MessageQueuePermission();
            Hashtable hashtable = new Hashtable(GetComparer());
            permission2.resolvedFormatNames = hashtable;
            if (this.resolvedFormatNames.Count < permission.resolvedFormatNames.Count)
            {
                enumerator = this.resolvedFormatNames.GetEnumerator();
                resolvedFormatNames = permission.resolvedFormatNames;
            }
            else
            {
                enumerator = permission.resolvedFormatNames.GetEnumerator();
                resolvedFormatNames = this.resolvedFormatNames;
            }
            while (enumerator.MoveNext())
            {
                if (resolvedFormatNames.ContainsKey(enumerator.Key))
                {
                    string key = (string) enumerator.Key;
                    MessageQueuePermissionAccess access = (MessageQueuePermissionAccess) enumerator.Value;
                    MessageQueuePermissionAccess access2 = (MessageQueuePermissionAccess) resolvedFormatNames[key];
                    hashtable.Add(key, access & access2);
                }
            }
            return permission2;
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return false;
            }
            if (!(target is MessageQueuePermission))
            {
                throw new ArgumentException(Res.GetString("InvalidParameter", new object[] { "target", target.ToString() }));
            }
            MessageQueuePermission permission = (MessageQueuePermission) target;
            if (!permission.IsUnrestricted())
            {
                IDictionaryEnumerator enumerator;
                if (this.IsUnrestricted())
                {
                    return false;
                }
                this.ResolveFormatNames();
                permission.ResolveFormatNames();
                if (((this.resolvedFormatNames.Count == 0) && (permission.resolvedFormatNames.Count != 0)) || ((this.resolvedFormatNames.Count != 0) && (permission.resolvedFormatNames.Count == 0)))
                {
                    return false;
                }
                if (permission.resolvedFormatNames.ContainsKey("*"))
                {
                    MessageQueuePermissionAccess access = (MessageQueuePermissionAccess) permission.resolvedFormatNames["*"];
                    enumerator = this.resolvedFormatNames.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        MessageQueuePermissionAccess access2 = (MessageQueuePermissionAccess) enumerator.Value;
                        if ((access2 & access) != access2)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                enumerator = this.resolvedFormatNames.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string key = (string) enumerator.Key;
                    if (!permission.resolvedFormatNames.ContainsKey(key))
                    {
                        return false;
                    }
                    MessageQueuePermissionAccess access3 = (MessageQueuePermissionAccess) enumerator.Value;
                    MessageQueuePermissionAccess access4 = (MessageQueuePermissionAccess) permission.resolvedFormatNames[key];
                    if ((access3 & access4) != access3)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsUnrestricted()
        {
            return this.isUnrestricted;
        }

        internal void ResolveFormatNames()
        {
            if (this.resolvedFormatNames == null)
            {
                this.resolvedFormatNames = new Hashtable(GetComparer());
                IEnumerator enumerator = this.PermissionEntries.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    MessageQueuePermissionEntry current = (MessageQueuePermissionEntry) enumerator.Current;
                    if (current.Path != null)
                    {
                        if (current.Path == "*")
                        {
                            this.resolvedFormatNames.Add("*", current.PermissionAccess);
                        }
                        else
                        {
                            try
                            {
                                MessageQueue queue = new MessageQueue(current.Path);
                                this.resolvedFormatNames.Add(queue.FormatName, current.PermissionAccess);
                            }
                            catch
                            {
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            MessageQueueCriteria criteria = new MessageQueueCriteria();
                            if (current.MachineName != null)
                            {
                                criteria.MachineName = current.MachineName;
                            }
                            if (current.Category != null)
                            {
                                criteria.Category = new Guid(current.Category);
                            }
                            if (current.Label != null)
                            {
                                criteria.Label = current.Label;
                            }
                            IEnumerator messageQueueEnumerator = MessageQueue.GetMessageQueueEnumerator(criteria, false);
                            while (messageQueueEnumerator.MoveNext())
                            {
                                MessageQueue queue2 = (MessageQueue) messageQueueEnumerator.Current;
                                this.resolvedFormatNames.Add(queue2.FormatName, current.PermissionAccess);
                            }
                            continue;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            Type type = base.GetType();
            element.AddAttribute("class", type.FullName + ", " + type.Module.Assembly.FullName.Replace('"', '\''));
            element.AddAttribute("version", "1");
            if (this.isUnrestricted)
            {
                element.AddAttribute("Unrestricted", "true");
                return element;
            }
            IEnumerator enumerator = this.PermissionEntries.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SecurityElement child = null;
                MessageQueuePermissionEntry current = (MessageQueuePermissionEntry) enumerator.Current;
                if (current.Path != null)
                {
                    child = new SecurityElement("Path");
                    child.AddAttribute("value", current.Path);
                }
                else
                {
                    child = new SecurityElement("Criteria");
                    if (current.MachineName != null)
                    {
                        child.AddAttribute("machine", current.MachineName);
                    }
                    if (current.Category != null)
                    {
                        child.AddAttribute("category", current.Category);
                    }
                    if (current.Label != null)
                    {
                        child.AddAttribute("label", current.Label);
                    }
                }
                int permissionAccess = (int) current.PermissionAccess;
                if (permissionAccess != 0)
                {
                    StringBuilder builder = null;
                    int[] values = (int[]) Enum.GetValues(typeof(MessageQueuePermissionAccess));
                    Array.Sort(values, System.InvariantComparer.Default);
                    for (int i = values.Length - 1; i >= 0; i--)
                    {
                        if ((values[i] != 0) && ((permissionAccess & values[i]) == values[i]))
                        {
                            if (builder == null)
                            {
                                builder = new StringBuilder();
                            }
                            else
                            {
                                builder.Append("|");
                            }
                            builder.Append(Enum.GetName(typeof(MessageQueuePermissionAccess), values[i]));
                            permissionAccess &= values[i] ^ values[i];
                        }
                    }
                    child.AddAttribute("access", builder.ToString());
                }
                element.AddChild(child);
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            if (!(target is MessageQueuePermission))
            {
                throw new ArgumentException(Res.GetString("InvalidParameter", new object[] { "target", target.ToString() }));
            }
            MessageQueuePermission permission = (MessageQueuePermission) target;
            MessageQueuePermission permission2 = new MessageQueuePermission();
            if (this.IsUnrestricted() || permission.IsUnrestricted())
            {
                permission2.isUnrestricted = true;
                return permission2;
            }
            Hashtable hashtable = new Hashtable(GetComparer());
            this.ResolveFormatNames();
            permission.ResolveFormatNames();
            IDictionaryEnumerator enumerator = this.resolvedFormatNames.GetEnumerator();
            IDictionaryEnumerator enumerator2 = permission.resolvedFormatNames.GetEnumerator();
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
                    MessageQueuePermissionAccess access = (MessageQueuePermissionAccess) hashtable[enumerator2.Key];
                    hashtable[enumerator2.Key] = access | ((MessageQueuePermissionAccess) enumerator2.Value);
                }
            }
            permission2.resolvedFormatNames = hashtable;
            return permission2;
        }

        public MessageQueuePermissionEntryCollection PermissionEntries
        {
            get
            {
                if (this.innerCollection == null)
                {
                    if (this.resolvedFormatNames == null)
                    {
                        this.innerCollection = new MessageQueuePermissionEntryCollection(this);
                    }
                    else
                    {
                        Hashtable resolvedFormatNames = this.resolvedFormatNames;
                        this.innerCollection = new MessageQueuePermissionEntryCollection(this);
                        foreach (string str in resolvedFormatNames.Keys)
                        {
                            string str2;
                            if (str == "*")
                            {
                                str2 = "*";
                            }
                            else
                            {
                                str2 = "FORMATNAME:" + str;
                            }
                            MessageQueuePermissionEntry entry = new MessageQueuePermissionEntry((MessageQueuePermissionAccess) resolvedFormatNames[str], str2);
                            this.innerCollection.Add(entry);
                        }
                    }
                }
                return this.innerCollection;
            }
        }
    }
}

