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
    public sealed class UrlIdentityPermission : CodeAccessPermission, IBuiltInPermission
    {
        [OptionalField(VersionAdded=2)]
        private string m_serializedPermission;
        [OptionalField(VersionAdded=2)]
        private bool m_unrestricted;
        private URLString m_url;
        [OptionalField(VersionAdded=2)]
        private URLString[] m_urls;

        public UrlIdentityPermission(PermissionState state)
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

        internal UrlIdentityPermission(URLString site)
        {
            this.m_unrestricted = false;
            this.m_urls = new URLString[] { site };
        }

        public UrlIdentityPermission(string site)
        {
            if (site == null)
            {
                throw new ArgumentNullException("site");
            }
            this.Url = site;
        }

        internal void AppendOrigin(ArrayList originList)
        {
            if (this.m_urls == null)
            {
                originList.Add("");
            }
            else
            {
                for (int i = 0; i < this.m_urls.Length; i++)
                {
                    originList.Add(this.m_urls[i].ToString());
                }
            }
        }

        public override IPermission Copy()
        {
            UrlIdentityPermission permission = new UrlIdentityPermission(PermissionState.None) {
                m_unrestricted = this.m_unrestricted
            };
            if (this.m_urls != null)
            {
                permission.m_urls = new URLString[this.m_urls.Length];
                for (int i = 0; i < this.m_urls.Length; i++)
                {
                    permission.m_urls[i] = (URLString) this.m_urls[i].Copy();
                }
            }
            return permission;
        }

        public override void FromXml(SecurityElement esd)
        {
            this.m_unrestricted = false;
            this.m_urls = null;
            CodeAccessPermission.ValidateElement(esd, this);
            string strA = esd.Attribute("Unrestricted");
            if ((strA != null) && (string.Compare(strA, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.m_unrestricted = true;
            }
            else
            {
                string url = esd.Attribute("Url");
                List<URLString> list = new List<URLString>();
                if (url != null)
                {
                    list.Add(new URLString(url, true));
                }
                ArrayList children = esd.Children;
                if (children != null)
                {
                    foreach (SecurityElement element in children)
                    {
                        url = element.Attribute("Url");
                        if (url != null)
                        {
                            list.Add(new URLString(url, true));
                        }
                    }
                }
                if (list.Count != 0)
                {
                    this.m_urls = list.ToArray();
                }
            }
        }

        internal static int GetTokenIndex()
        {
            return 13;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            UrlIdentityPermission permission = target as UrlIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.m_unrestricted && permission.m_unrestricted)
            {
                return new UrlIdentityPermission(PermissionState.None) { m_unrestricted = true };
            }
            if (this.m_unrestricted)
            {
                return permission.Copy();
            }
            if (permission.m_unrestricted)
            {
                return this.Copy();
            }
            if (((this.m_urls == null) || (permission.m_urls == null)) || ((this.m_urls.Length == 0) || (permission.m_urls.Length == 0)))
            {
                return null;
            }
            List<URLString> list = new List<URLString>();
            foreach (URLString str in this.m_urls)
            {
                foreach (URLString str2 in permission.m_urls)
                {
                    URLString item = (URLString) str.Intersect(str2);
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
            return new UrlIdentityPermission(PermissionState.None) { m_urls = list.ToArray() };
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                if (!this.m_unrestricted)
                {
                    if (this.m_urls == null)
                    {
                        return true;
                    }
                    if (this.m_urls.Length == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            UrlIdentityPermission permission = target as UrlIdentityPermission;
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
                if (this.m_urls != null)
                {
                    foreach (URLString str in this.m_urls)
                    {
                        bool flag = false;
                        if (permission.m_urls != null)
                        {
                            foreach (URLString str2 in permission.m_urls)
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
            else if (this.m_url != null)
            {
                this.m_unrestricted = false;
                this.m_urls = new URLString[] { this.m_url };
                this.m_url = null;
            }
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.m_serializedPermission = null;
                this.m_url = null;
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.m_serializedPermission = this.ToXml().ToString();
                if ((this.m_urls != null) && (this.m_urls.Length == 1))
                {
                    this.m_url = this.m_urls[0];
                }
            }
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.UrlIdentityPermission");
            if (this.m_unrestricted)
            {
                element.AddAttribute("Unrestricted", "true");
                return element;
            }
            if (this.m_urls != null)
            {
                if (this.m_urls.Length == 1)
                {
                    element.AddAttribute("Url", this.m_urls[0].ToString());
                    return element;
                }
                for (int i = 0; i < this.m_urls.Length; i++)
                {
                    SecurityElement child = new SecurityElement("Url");
                    child.AddAttribute("Url", this.m_urls[i].ToString());
                    element.AddChild(child);
                }
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                if (((this.m_urls == null) || (this.m_urls.Length == 0)) && !this.m_unrestricted)
                {
                    return null;
                }
                return this.Copy();
            }
            UrlIdentityPermission permission = target as UrlIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.m_unrestricted || permission.m_unrestricted)
            {
                return new UrlIdentityPermission(PermissionState.None) { m_unrestricted = true };
            }
            if ((this.m_urls == null) || (this.m_urls.Length == 0))
            {
                if ((permission.m_urls != null) && (permission.m_urls.Length != 0))
                {
                    return permission.Copy();
                }
                return null;
            }
            if ((permission.m_urls == null) || (permission.m_urls.Length == 0))
            {
                return this.Copy();
            }
            List<URLString> list = new List<URLString>();
            foreach (URLString str in this.m_urls)
            {
                list.Add(str);
            }
            foreach (URLString str2 in permission.m_urls)
            {
                bool flag = false;
                foreach (URLString str3 in list)
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
            return new UrlIdentityPermission(PermissionState.None) { m_urls = list.ToArray() };
        }

        public string Url
        {
            get
            {
                if (this.m_urls == null)
                {
                    return "";
                }
                if (this.m_urls.Length != 1)
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_AmbiguousIdentity"));
                }
                return this.m_urls[0].ToString();
            }
            set
            {
                this.m_unrestricted = false;
                if ((value == null) || (value.Length == 0))
                {
                    this.m_urls = null;
                }
                else
                {
                    this.m_urls = new URLString[] { new URLString(value) };
                }
            }
        }
    }
}

