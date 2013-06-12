namespace System.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class StrongNameIdentityPermission : CodeAccessPermission, IBuiltInPermission
    {
        private StrongName2[] m_strongNames;
        private bool m_unrestricted;

        public StrongNameIdentityPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.m_unrestricted = true;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.m_unrestricted = false;
            }
        }

        public StrongNameIdentityPermission(StrongNamePublicKeyBlob blob, string name, System.Version version)
        {
            if (blob == null)
            {
                throw new ArgumentNullException("blob");
            }
            if ((name != null) && name.Equals(""))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyStrongName"));
            }
            this.m_unrestricted = false;
            this.m_strongNames = new StrongName2[] { new StrongName2(blob, name, version) };
        }

        public override IPermission Copy()
        {
            StrongNameIdentityPermission permission = new StrongNameIdentityPermission(PermissionState.None) {
                m_unrestricted = this.m_unrestricted
            };
            if (this.m_strongNames != null)
            {
                permission.m_strongNames = new StrongName2[this.m_strongNames.Length];
                for (int i = 0; i < this.m_strongNames.Length; i++)
                {
                    permission.m_strongNames[i] = this.m_strongNames[i].Copy();
                }
            }
            return permission;
        }

        public override void FromXml(SecurityElement e)
        {
            this.m_unrestricted = false;
            this.m_strongNames = null;
            CodeAccessPermission.ValidateElement(e, this);
            string strA = e.Attribute("Unrestricted");
            if ((strA != null) && (string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.m_unrestricted = true;
            }
            else
            {
                StrongName2 name;
                string publicKey = e.Attribute("PublicKeyBlob");
                string str3 = e.Attribute("Name");
                string version = e.Attribute("AssemblyVersion");
                List<StrongName2> list = new List<StrongName2>();
                if (((publicKey != null) || (str3 != null)) || (version != null))
                {
                    name = new StrongName2((publicKey == null) ? null : new StrongNamePublicKeyBlob(publicKey), str3, (version == null) ? null : new System.Version(version));
                    list.Add(name);
                }
                ArrayList children = e.Children;
                if (children != null)
                {
                    foreach (SecurityElement element in children)
                    {
                        publicKey = element.Attribute("PublicKeyBlob");
                        str3 = element.Attribute("Name");
                        version = element.Attribute("AssemblyVersion");
                        if (((publicKey != null) || (str3 != null)) || (version != null))
                        {
                            name = new StrongName2((publicKey == null) ? null : new StrongNamePublicKeyBlob(publicKey), str3, (version == null) ? null : new System.Version(version));
                            list.Add(name);
                        }
                    }
                }
                if (list.Count != 0)
                {
                    this.m_strongNames = list.ToArray();
                }
            }
        }

        internal static int GetTokenIndex()
        {
            return 12;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            StrongNameIdentityPermission permission = target as StrongNameIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.m_unrestricted && permission.m_unrestricted)
            {
                return new StrongNameIdentityPermission(PermissionState.None) { m_unrestricted = true };
            }
            if (this.m_unrestricted)
            {
                return permission.Copy();
            }
            if (permission.m_unrestricted)
            {
                return this.Copy();
            }
            if (((this.m_strongNames == null) || (permission.m_strongNames == null)) || ((this.m_strongNames.Length == 0) || (permission.m_strongNames.Length == 0)))
            {
                return null;
            }
            List<StrongName2> list = new List<StrongName2>();
            foreach (StrongName2 name in this.m_strongNames)
            {
                foreach (StrongName2 name2 in permission.m_strongNames)
                {
                    StrongName2 item = name.Intersect(name2);
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            return new StrongNameIdentityPermission(PermissionState.None) { m_strongNames = list.ToArray() };
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                if (!this.m_unrestricted)
                {
                    if (this.m_strongNames == null)
                    {
                        return true;
                    }
                    if (this.m_strongNames.Length == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            StrongNameIdentityPermission permission = target as StrongNameIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (!permission.m_unrestricted)
            {
                if (this.m_unrestricted)
                {
                    return false;
                }
                if (this.m_strongNames != null)
                {
                    foreach (StrongName2 name in this.m_strongNames)
                    {
                        bool flag = false;
                        if (permission.m_strongNames != null)
                        {
                            foreach (StrongName2 name2 in permission.m_strongNames)
                            {
                                if (name.IsSubsetOf(name2))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            return false;
                        }
                    }
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
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.StrongNameIdentityPermission");
            if (this.m_unrestricted)
            {
                element.AddAttribute("Unrestricted", "true");
                return element;
            }
            if (this.m_strongNames != null)
            {
                if (this.m_strongNames.Length == 1)
                {
                    if (this.m_strongNames[0].m_publicKeyBlob != null)
                    {
                        element.AddAttribute("PublicKeyBlob", Hex.EncodeHexString(this.m_strongNames[0].m_publicKeyBlob.PublicKey));
                    }
                    if (this.m_strongNames[0].m_name != null)
                    {
                        element.AddAttribute("Name", this.m_strongNames[0].m_name);
                    }
                    if (this.m_strongNames[0].m_version != null)
                    {
                        element.AddAttribute("AssemblyVersion", this.m_strongNames[0].m_version.ToString());
                    }
                    return element;
                }
                for (int i = 0; i < this.m_strongNames.Length; i++)
                {
                    SecurityElement child = new SecurityElement("StrongName");
                    if (this.m_strongNames[i].m_publicKeyBlob != null)
                    {
                        child.AddAttribute("PublicKeyBlob", Hex.EncodeHexString(this.m_strongNames[i].m_publicKeyBlob.PublicKey));
                    }
                    if (this.m_strongNames[i].m_name != null)
                    {
                        child.AddAttribute("Name", this.m_strongNames[i].m_name);
                    }
                    if (this.m_strongNames[i].m_version != null)
                    {
                        child.AddAttribute("AssemblyVersion", this.m_strongNames[i].m_version.ToString());
                    }
                    element.AddChild(child);
                }
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                if (((this.m_strongNames == null) || (this.m_strongNames.Length == 0)) && !this.m_unrestricted)
                {
                    return null;
                }
                return this.Copy();
            }
            StrongNameIdentityPermission permission = target as StrongNameIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.m_unrestricted || permission.m_unrestricted)
            {
                return new StrongNameIdentityPermission(PermissionState.None) { m_unrestricted = true };
            }
            if ((this.m_strongNames == null) || (this.m_strongNames.Length == 0))
            {
                if ((permission.m_strongNames != null) && (permission.m_strongNames.Length != 0))
                {
                    return permission.Copy();
                }
                return null;
            }
            if ((permission.m_strongNames == null) || (permission.m_strongNames.Length == 0))
            {
                return this.Copy();
            }
            List<StrongName2> list = new List<StrongName2>();
            foreach (StrongName2 name in this.m_strongNames)
            {
                list.Add(name);
            }
            foreach (StrongName2 name2 in permission.m_strongNames)
            {
                bool flag = false;
                foreach (StrongName2 name3 in list)
                {
                    if (name2.Equals(name3))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    list.Add(name2);
                }
            }
            return new StrongNameIdentityPermission(PermissionState.None) { m_strongNames = list.ToArray() };
        }

        public string Name
        {
            get
            {
                if ((this.m_strongNames == null) || (this.m_strongNames.Length == 0))
                {
                    return "";
                }
                if (this.m_strongNames.Length > 1)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_AmbiguousIdentity"));
                }
                return this.m_strongNames[0].m_name;
            }
            set
            {
                if ((value != null) && (value.Length == 0))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"));
                }
                this.m_unrestricted = false;
                if ((this.m_strongNames != null) && (this.m_strongNames.Length == 1))
                {
                    this.m_strongNames[0].m_name = value;
                }
                else
                {
                    this.m_strongNames = new StrongName2[] { new StrongName2(null, value, new System.Version()) };
                }
            }
        }

        public StrongNamePublicKeyBlob PublicKey
        {
            get
            {
                if ((this.m_strongNames == null) || (this.m_strongNames.Length == 0))
                {
                    return null;
                }
                if (this.m_strongNames.Length > 1)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_AmbiguousIdentity"));
                }
                return this.m_strongNames[0].m_publicKeyBlob;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("PublicKey");
                }
                this.m_unrestricted = false;
                if ((this.m_strongNames != null) && (this.m_strongNames.Length == 1))
                {
                    this.m_strongNames[0].m_publicKeyBlob = value;
                }
                else
                {
                    this.m_strongNames = new StrongName2[] { new StrongName2(value, "", new System.Version()) };
                }
            }
        }

        public System.Version Version
        {
            get
            {
                if ((this.m_strongNames == null) || (this.m_strongNames.Length == 0))
                {
                    return new System.Version();
                }
                if (this.m_strongNames.Length > 1)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_AmbiguousIdentity"));
                }
                return this.m_strongNames[0].m_version;
            }
            set
            {
                this.m_unrestricted = false;
                if ((this.m_strongNames != null) && (this.m_strongNames.Length == 1))
                {
                    this.m_strongNames[0].m_version = value;
                }
                else
                {
                    this.m_strongNames = new StrongName2[] { new StrongName2(null, "", value) };
                }
            }
        }
    }
}

