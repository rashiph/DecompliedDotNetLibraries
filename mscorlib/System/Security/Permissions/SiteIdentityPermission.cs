namespace System.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class SiteIdentityPermission : CodeAccessPermission, IBuiltInPermission
    {
        [OptionalField(VersionAdded=2)]
        private string m_serializedPermission;
        private SiteString m_site;
        [OptionalField(VersionAdded=2)]
        private SiteString[] m_sites;
        [OptionalField(VersionAdded=2)]
        private bool m_unrestricted;

        public SiteIdentityPermission(PermissionState state)
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

        public SiteIdentityPermission(string site)
        {
            this.Site = site;
        }

        public override IPermission Copy()
        {
            SiteIdentityPermission permission = new SiteIdentityPermission(PermissionState.None) {
                m_unrestricted = this.m_unrestricted
            };
            if (this.m_sites != null)
            {
                permission.m_sites = new SiteString[this.m_sites.Length];
                for (int i = 0; i < this.m_sites.Length; i++)
                {
                    permission.m_sites[i] = this.m_sites[i].Copy();
                }
            }
            return permission;
        }

        public override void FromXml(SecurityElement esd)
        {
            this.m_unrestricted = false;
            this.m_sites = null;
            CodeAccessPermission.ValidateElement(esd, this);
            string strA = esd.Attribute("Unrestricted");
            if ((strA != null) && (string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.m_unrestricted = true;
            }
            else
            {
                string site = esd.Attribute("Site");
                List<SiteString> list = new List<SiteString>();
                if (site != null)
                {
                    list.Add(new SiteString(site));
                }
                ArrayList children = esd.Children;
                if (children != null)
                {
                    foreach (SecurityElement element in children)
                    {
                        site = element.Attribute("Site");
                        if (site != null)
                        {
                            list.Add(new SiteString(site));
                        }
                    }
                }
                if (list.Count != 0)
                {
                    this.m_sites = list.ToArray();
                }
            }
        }

        internal static int GetTokenIndex()
        {
            return 11;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            SiteIdentityPermission permission = target as SiteIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.m_unrestricted && permission.m_unrestricted)
            {
                return new SiteIdentityPermission(PermissionState.None) { m_unrestricted = true };
            }
            if (this.m_unrestricted)
            {
                return permission.Copy();
            }
            if (permission.m_unrestricted)
            {
                return this.Copy();
            }
            if (((this.m_sites == null) || (permission.m_sites == null)) || ((this.m_sites.Length == 0) || (permission.m_sites.Length == 0)))
            {
                return null;
            }
            List<SiteString> list = new List<SiteString>();
            foreach (SiteString str in this.m_sites)
            {
                foreach (SiteString str2 in permission.m_sites)
                {
                    SiteString item = str.Intersect(str2);
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
            return new SiteIdentityPermission(PermissionState.None) { m_sites = list.ToArray() };
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                if (!this.m_unrestricted)
                {
                    if (this.m_sites == null)
                    {
                        return true;
                    }
                    if (this.m_sites.Length == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            SiteIdentityPermission permission = target as SiteIdentityPermission;
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
                if (this.m_sites != null)
                {
                    foreach (SiteString str in this.m_sites)
                    {
                        bool flag = false;
                        if (permission.m_sites != null)
                        {
                            foreach (SiteString str2 in permission.m_sites)
                            {
                                if (str.IsSubsetOf(str2))
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

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (this.m_serializedPermission != null)
            {
                this.FromXml(SecurityElement.FromString(this.m_serializedPermission));
                this.m_serializedPermission = null;
            }
            else if (this.m_site != null)
            {
                this.m_unrestricted = false;
                this.m_sites = new SiteString[] { this.m_site };
                this.m_site = null;
            }
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.m_serializedPermission = null;
                this.m_site = null;
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.m_serializedPermission = this.ToXml().ToString();
                if ((this.m_sites != null) && (this.m_sites.Length == 1))
                {
                    this.m_site = this.m_sites[0];
                }
            }
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.SiteIdentityPermission");
            if (this.m_unrestricted)
            {
                element.AddAttribute("Unrestricted", "true");
                return element;
            }
            if (this.m_sites != null)
            {
                if (this.m_sites.Length == 1)
                {
                    element.AddAttribute("Site", this.m_sites[0].ToString());
                    return element;
                }
                for (int i = 0; i < this.m_sites.Length; i++)
                {
                    SecurityElement child = new SecurityElement("Site");
                    child.AddAttribute("Site", this.m_sites[i].ToString());
                    element.AddChild(child);
                }
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                if (((this.m_sites == null) || (this.m_sites.Length == 0)) && !this.m_unrestricted)
                {
                    return null;
                }
                return this.Copy();
            }
            SiteIdentityPermission permission = target as SiteIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.m_unrestricted || permission.m_unrestricted)
            {
                return new SiteIdentityPermission(PermissionState.None) { m_unrestricted = true };
            }
            if ((this.m_sites == null) || (this.m_sites.Length == 0))
            {
                if ((permission.m_sites != null) && (permission.m_sites.Length != 0))
                {
                    return permission.Copy();
                }
                return null;
            }
            if ((permission.m_sites == null) || (permission.m_sites.Length == 0))
            {
                return this.Copy();
            }
            List<SiteString> list = new List<SiteString>();
            foreach (SiteString str in this.m_sites)
            {
                list.Add(str);
            }
            foreach (SiteString str2 in permission.m_sites)
            {
                bool flag = false;
                foreach (SiteString str3 in list)
                {
                    if (str2.Equals(str3))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    list.Add(str2);
                }
            }
            return new SiteIdentityPermission(PermissionState.None) { m_sites = list.ToArray() };
        }

        public string Site
        {
            get
            {
                if (this.m_sites == null)
                {
                    return "";
                }
                if (this.m_sites.Length != 1)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_AmbiguousIdentity"));
                }
                return this.m_sites[0].ToString();
            }
            set
            {
                this.m_unrestricted = false;
                this.m_sites = new SiteString[] { new SiteString(value) };
            }
        }
    }
}

