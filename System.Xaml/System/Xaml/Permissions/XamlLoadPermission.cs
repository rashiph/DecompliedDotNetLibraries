namespace System.Xaml.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Xaml;

    [Serializable]
    public sealed class XamlLoadPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private bool _isUnrestricted;
        private static IList<XamlAccessLevel> s_emptyAccessLevel;

        public XamlLoadPermission(PermissionState state)
        {
            this.Init(state == PermissionState.Unrestricted, null);
        }

        public XamlLoadPermission(XamlAccessLevel allowedAccess)
        {
            if (allowedAccess == null)
            {
                throw new ArgumentNullException("allowedAccess");
            }
            this.Init(false, new XamlAccessLevel[] { allowedAccess });
        }

        public XamlLoadPermission(IEnumerable<XamlAccessLevel> allowedAccess)
        {
            if (allowedAccess == null)
            {
                throw new ArgumentNullException("allowedAccess");
            }
            List<XamlAccessLevel> list = new List<XamlAccessLevel>(allowedAccess);
            foreach (XamlAccessLevel level in allowedAccess)
            {
                if (level == null)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("CollectionCannotContainNulls", new object[] { "allowedAccess" }));
                }
                list.Add(level);
            }
            this.Init(false, list);
        }

        private XamlLoadPermission(XamlLoadPermission other)
        {
            this._isUnrestricted = other._isUnrestricted;
            this.AllowedAccess = other.AllowedAccess;
        }

        private static XamlLoadPermission CastPermission(IPermission other, string argName)
        {
            XamlLoadPermission permission = other as XamlLoadPermission;
            if (permission == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("ExpectedLoadPermission"), argName);
            }
            return permission;
        }

        public override IPermission Copy()
        {
            return new XamlLoadPermission(this);
        }

        public override void FromXml(SecurityElement elem)
        {
            if (elem == null)
            {
                throw new ArgumentNullException("elem");
            }
            if (elem.Tag != "IPermission")
            {
                throw new ArgumentException(System.Xaml.SR.Get("SecurityXmlUnexpectedTag", new object[] { elem.Tag, "IPermission" }), "elem");
            }
            string str = elem.Attribute("class");
            if (!str.StartsWith(base.GetType().FullName, false, TypeConverterHelper.InvariantEnglishUS))
            {
                throw new ArgumentException(System.Xaml.SR.Get("SecurityXmlUnexpectedValue", new object[] { str, "class", base.GetType().FullName }), "elem");
            }
            string str2 = elem.Attribute("version");
            if ((str2 != null) && (str2 != "1"))
            {
                throw new ArgumentException(System.Xaml.SR.Get("SecurityXmlUnexpectedValue", new object[] { str, "version", "1" }), "elem");
            }
            string str3 = elem.Attribute("Unrestricted");
            if ((str3 != null) && bool.Parse(str3))
            {
                this.Init(true, null);
            }
            else
            {
                List<XamlAccessLevel> allowedAccess = null;
                if (elem.Children != null)
                {
                    allowedAccess = new List<XamlAccessLevel>(elem.Children.Count);
                    foreach (SecurityElement element in elem.Children)
                    {
                        allowedAccess.Add(XamlAccessLevel.FromXml(element));
                    }
                }
                this.Init(false, allowedAccess);
            }
        }

        public bool Includes(XamlAccessLevel requestedAccess)
        {
            if (requestedAccess == null)
            {
                throw new ArgumentNullException("requestedAccess");
            }
            if (this._isUnrestricted)
            {
                return true;
            }
            foreach (XamlAccessLevel level in this.AllowedAccess)
            {
                if (level.Includes(requestedAccess))
                {
                    return true;
                }
            }
            return false;
        }

        private void Init(bool isUnrestricted, IList<XamlAccessLevel> allowedAccess)
        {
            this._isUnrestricted = isUnrestricted;
            if (allowedAccess == null)
            {
                if (s_emptyAccessLevel == null)
                {
                    s_emptyAccessLevel = new ReadOnlyCollection<XamlAccessLevel>(new XamlAccessLevel[0]);
                }
                this.AllowedAccess = s_emptyAccessLevel;
            }
            else
            {
                this.AllowedAccess = new ReadOnlyCollection<XamlAccessLevel>(allowedAccess);
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            XamlLoadPermission permission = CastPermission(target, "target");
            if (permission.IsUnrestricted())
            {
                return this.Copy();
            }
            if (this.IsUnrestricted())
            {
                return permission.Copy();
            }
            List<XamlAccessLevel> allowedAccess = new List<XamlAccessLevel>();
            foreach (XamlAccessLevel level in this.AllowedAccess)
            {
                if (permission.Includes(level))
                {
                    allowedAccess.Add(level);
                }
                else if (level.PrivateAccessToTypeName != null)
                {
                    XamlAccessLevel requestedAccess = level.AssemblyOnly();
                    if (permission.Includes(requestedAccess))
                    {
                        allowedAccess.Add(requestedAccess);
                    }
                }
            }
            return new XamlLoadPermission(allowedAccess);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return (!this.IsUnrestricted() && (this.AllowedAccess.Count == 0));
            }
            XamlLoadPermission permission = CastPermission(target, "target");
            if (!permission.IsUnrestricted())
            {
                if (this.IsUnrestricted())
                {
                    return false;
                }
                foreach (XamlAccessLevel level in this.AllowedAccess)
                {
                    if (!permission.Includes(level))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsUnrestricted()
        {
            return this._isUnrestricted;
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            element.AddAttribute("class", base.GetType().AssemblyQualifiedName);
            element.AddAttribute("version", "1");
            if (this.IsUnrestricted())
            {
                element.AddAttribute("Unrestricted", bool.TrueString);
                return element;
            }
            foreach (XamlAccessLevel level in this.AllowedAccess)
            {
                element.AddChild(level.ToXml());
            }
            return element;
        }

        public override IPermission Union(IPermission other)
        {
            if (other == null)
            {
                return this.Copy();
            }
            XamlLoadPermission permission = CastPermission(other, "other");
            if (this.IsUnrestricted() || permission.IsUnrestricted())
            {
                return new XamlLoadPermission(PermissionState.Unrestricted);
            }
            List<XamlAccessLevel> allowedAccess = new List<XamlAccessLevel>(this.AllowedAccess);
            foreach (XamlAccessLevel level in permission.AllowedAccess)
            {
                if (!this.Includes(level))
                {
                    allowedAccess.Add(level);
                    if (level.PrivateAccessToTypeName != null)
                    {
                        for (int i = 0; i < allowedAccess.Count; i++)
                        {
                            if ((allowedAccess[i].PrivateAccessToTypeName == null) && (allowedAccess[i].AssemblyNameString == level.AssemblyNameString))
                            {
                                allowedAccess.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }
            return new XamlLoadPermission(allowedAccess);
        }

        public IList<XamlAccessLevel> AllowedAccess { get; private set; }

        private static class XmlConstants
        {
            public const string Class = "class";
            public const string IPermission = "IPermission";
            public const string Unrestricted = "Unrestricted";
            public const string Version = "version";
            public const string VersionNumber = "1";
        }
    }
}

