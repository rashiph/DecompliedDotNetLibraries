namespace System.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class KeyContainerPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
    {
        private KeyContainerPermissionAccessEntryCollection m_accessEntries;
        private KeyContainerPermissionFlags m_flags;

        public KeyContainerPermission(KeyContainerPermissionFlags flags)
        {
            VerifyFlags(flags);
            this.m_flags = flags;
            this.m_accessEntries = new KeyContainerPermissionAccessEntryCollection(this.m_flags);
        }

        public KeyContainerPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.m_flags = KeyContainerPermissionFlags.AllFlags;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.m_flags = KeyContainerPermissionFlags.NoFlags;
            }
            this.m_accessEntries = new KeyContainerPermissionAccessEntryCollection(this.m_flags);
        }

        public KeyContainerPermission(KeyContainerPermissionFlags flags, KeyContainerPermissionAccessEntry[] accessList)
        {
            if (accessList == null)
            {
                throw new ArgumentNullException("accessList");
            }
            VerifyFlags(flags);
            this.m_flags = flags;
            this.m_accessEntries = new KeyContainerPermissionAccessEntryCollection(this.m_flags);
            for (int i = 0; i < accessList.Length; i++)
            {
                this.m_accessEntries.Add(accessList[i]);
            }
        }

        private void AddAccessEntries(SecurityElement securityElement)
        {
            if ((securityElement.InternalChildren != null) && (securityElement.InternalChildren.Count != 0))
            {
                IEnumerator enumerator = securityElement.Children.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    SecurityElement current = (SecurityElement) enumerator.Current;
                    if ((current != null) && string.Equals(current.Tag, "AccessEntry"))
                    {
                        int count = current.m_lAttributes.Count;
                        string keyStore = null;
                        string providerName = null;
                        int providerType = -1;
                        string keyContainerName = null;
                        int keySpec = -1;
                        KeyContainerPermissionFlags noFlags = KeyContainerPermissionFlags.NoFlags;
                        for (int i = 0; i < count; i += 2)
                        {
                            string a = (string) current.m_lAttributes[i];
                            string str5 = (string) current.m_lAttributes[i + 1];
                            if (string.Equals(a, "KeyStore"))
                            {
                                keyStore = str5;
                            }
                            if (string.Equals(a, "ProviderName"))
                            {
                                providerName = str5;
                            }
                            else if (string.Equals(a, "ProviderType"))
                            {
                                providerType = Convert.ToInt32(str5, (IFormatProvider) null);
                            }
                            else if (string.Equals(a, "KeyContainerName"))
                            {
                                keyContainerName = str5;
                            }
                            else if (string.Equals(a, "KeySpec"))
                            {
                                keySpec = Convert.ToInt32(str5, (IFormatProvider) null);
                            }
                            else if (string.Equals(a, "Flags"))
                            {
                                noFlags = (KeyContainerPermissionFlags) Enum.Parse(typeof(KeyContainerPermissionFlags), str5);
                            }
                        }
                        KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(keyStore, providerName, providerType, keyContainerName, keySpec, noFlags);
                        this.AccessEntries.Add(accessEntry);
                    }
                }
            }
        }

        private void AddAccessEntryAndIntersect(KeyContainerPermissionAccessEntry accessEntry, KeyContainerPermission target)
        {
            KeyContainerPermissionAccessEntry entry;
            entry = new KeyContainerPermissionAccessEntry(accessEntry) {
                Flags = entry.Flags & GetApplicableFlags(accessEntry, target)
            };
            this.AccessEntries.Add(entry);
        }

        private void AddAccessEntryAndUnion(KeyContainerPermissionAccessEntry accessEntry, KeyContainerPermission target)
        {
            KeyContainerPermissionAccessEntry entry;
            entry = new KeyContainerPermissionAccessEntry(accessEntry) {
                Flags = entry.Flags | GetApplicableFlags(accessEntry, target)
            };
            this.AccessEntries.Add(entry);
        }

        public override IPermission Copy()
        {
            if (this.IsEmpty())
            {
                return null;
            }
            KeyContainerPermission permission = new KeyContainerPermission(this.m_flags);
            KeyContainerPermissionAccessEntryEnumerator enumerator = this.AccessEntries.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyContainerPermissionAccessEntry current = enumerator.Current;
                permission.AccessEntries.Add(current);
            }
            return permission;
        }

        public override void FromXml(SecurityElement securityElement)
        {
            CodeAccessPermission.ValidateElement(securityElement, this);
            if (XMLUtil.IsUnrestricted(securityElement))
            {
                this.m_flags = KeyContainerPermissionFlags.AllFlags;
                this.m_accessEntries = new KeyContainerPermissionAccessEntryCollection(this.m_flags);
            }
            else
            {
                this.m_flags = KeyContainerPermissionFlags.NoFlags;
                string str = securityElement.Attribute("Flags");
                if (str != null)
                {
                    KeyContainerPermissionFlags flags = (KeyContainerPermissionFlags) Enum.Parse(typeof(KeyContainerPermissionFlags), str);
                    VerifyFlags(flags);
                    this.m_flags = flags;
                }
                this.m_accessEntries = new KeyContainerPermissionAccessEntryCollection(this.m_flags);
                if ((securityElement.InternalChildren != null) && (securityElement.InternalChildren.Count != 0))
                {
                    IEnumerator enumerator = securityElement.Children.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        SecurityElement current = (SecurityElement) enumerator.Current;
                        if ((current != null) && string.Equals(current.Tag, "AccessList"))
                        {
                            this.AddAccessEntries(current);
                        }
                    }
                }
            }
        }

        private static KeyContainerPermissionFlags GetApplicableFlags(KeyContainerPermissionAccessEntry accessEntry, KeyContainerPermission target)
        {
            KeyContainerPermissionFlags noFlags = KeyContainerPermissionFlags.NoFlags;
            bool flag = true;
            int index = target.AccessEntries.IndexOf(accessEntry);
            if (index != -1)
            {
                return target.AccessEntries[index].Flags;
            }
            KeyContainerPermissionAccessEntryEnumerator enumerator = target.AccessEntries.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyContainerPermissionAccessEntry current = enumerator.Current;
                if (accessEntry.IsSubsetOf(current))
                {
                    if (!flag)
                    {
                        noFlags &= current.Flags;
                    }
                    else
                    {
                        noFlags = current.Flags;
                        flag = false;
                    }
                }
            }
            if (flag)
            {
                noFlags = target.Flags;
            }
            return noFlags;
        }

        private static int GetTokenIndex()
        {
            return 0x10;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target != null)
            {
                if (!base.VerifyType(target))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
                }
                KeyContainerPermission permission = (KeyContainerPermission) target;
                if (this.IsEmpty() || permission.IsEmpty())
                {
                    return null;
                }
                KeyContainerPermissionFlags flags = permission.m_flags & this.m_flags;
                KeyContainerPermission permission2 = new KeyContainerPermission(flags);
                KeyContainerPermissionAccessEntryEnumerator enumerator = this.AccessEntries.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyContainerPermissionAccessEntry current = enumerator.Current;
                    permission2.AddAccessEntryAndIntersect(current, permission);
                }
                KeyContainerPermissionAccessEntryEnumerator enumerator2 = permission.AccessEntries.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    KeyContainerPermissionAccessEntry accessEntry = enumerator2.Current;
                    permission2.AddAccessEntryAndIntersect(accessEntry, this);
                }
                if (!permission2.IsEmpty())
                {
                    return permission2;
                }
            }
            return null;
        }

        private bool IsEmpty()
        {
            if (this.Flags != KeyContainerPermissionFlags.NoFlags)
            {
                return false;
            }
            KeyContainerPermissionAccessEntryEnumerator enumerator = this.AccessEntries.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Flags != KeyContainerPermissionFlags.NoFlags)
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
                return this.IsEmpty();
            }
            if (!base.VerifyType(target))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            KeyContainerPermission permission = (KeyContainerPermission) target;
            if ((this.m_flags & permission.m_flags) != this.m_flags)
            {
                return false;
            }
            KeyContainerPermissionAccessEntryEnumerator enumerator = this.AccessEntries.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyContainerPermissionAccessEntry current = enumerator.Current;
                KeyContainerPermissionFlags applicableFlags = GetApplicableFlags(current, permission);
                if ((current.Flags & applicableFlags) != current.Flags)
                {
                    return false;
                }
            }
            KeyContainerPermissionAccessEntryEnumerator enumerator2 = permission.AccessEntries.GetEnumerator();
            while (enumerator2.MoveNext())
            {
                KeyContainerPermissionAccessEntry accessEntry = enumerator2.Current;
                KeyContainerPermissionFlags flags2 = GetApplicableFlags(accessEntry, this);
                if ((flags2 & accessEntry.Flags) != flags2)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsUnrestricted()
        {
            if (this.m_flags != KeyContainerPermissionFlags.AllFlags)
            {
                return false;
            }
            KeyContainerPermissionAccessEntryEnumerator enumerator = this.AccessEntries.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if ((enumerator.Current.Flags & KeyContainerPermissionFlags.AllFlags) != KeyContainerPermissionFlags.AllFlags)
                {
                    return false;
                }
            }
            return true;
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.KeyContainerPermission");
            if (!this.IsUnrestricted())
            {
                element.AddAttribute("Flags", this.m_flags.ToString());
                if (this.AccessEntries.Count > 0)
                {
                    SecurityElement child = new SecurityElement("AccessList");
                    KeyContainerPermissionAccessEntryEnumerator enumerator = this.AccessEntries.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        KeyContainerPermissionAccessEntry current = enumerator.Current;
                        SecurityElement element3 = new SecurityElement("AccessEntry");
                        element3.AddAttribute("KeyStore", current.KeyStore);
                        element3.AddAttribute("ProviderName", current.ProviderName);
                        element3.AddAttribute("ProviderType", current.ProviderType.ToString(null, null));
                        element3.AddAttribute("KeyContainerName", current.KeyContainerName);
                        element3.AddAttribute("KeySpec", current.KeySpec.ToString(null, null));
                        element3.AddAttribute("Flags", current.Flags.ToString());
                        child.AddChild(element3);
                    }
                    element.AddChild(child);
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
            if (!base.VerifyType(target))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            KeyContainerPermission permission = (KeyContainerPermission) target;
            if (this.IsUnrestricted() || permission.IsUnrestricted())
            {
                return new KeyContainerPermission(PermissionState.Unrestricted);
            }
            KeyContainerPermissionFlags flags = this.m_flags | permission.m_flags;
            KeyContainerPermission permission2 = new KeyContainerPermission(flags);
            KeyContainerPermissionAccessEntryEnumerator enumerator = this.AccessEntries.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyContainerPermissionAccessEntry current = enumerator.Current;
                permission2.AddAccessEntryAndUnion(current, permission);
            }
            KeyContainerPermissionAccessEntryEnumerator enumerator2 = permission.AccessEntries.GetEnumerator();
            while (enumerator2.MoveNext())
            {
                KeyContainerPermissionAccessEntry accessEntry = enumerator2.Current;
                permission2.AddAccessEntryAndUnion(accessEntry, this);
            }
            if (!permission2.IsEmpty())
            {
                return permission2;
            }
            return null;
        }

        internal static void VerifyFlags(KeyContainerPermissionFlags flags)
        {
            if ((flags & ~KeyContainerPermissionFlags.AllFlags) != KeyContainerPermissionFlags.NoFlags)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) flags }));
            }
        }

        public KeyContainerPermissionAccessEntryCollection AccessEntries
        {
            get
            {
                return this.m_accessEntries;
            }
        }

        public KeyContainerPermissionFlags Flags
        {
            get
            {
                return this.m_flags;
            }
        }
    }
}

