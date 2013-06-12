namespace System.Security.Permissions
{
    using System;
    using System.Security;
    using System.Security.Util;

    [Serializable]
    internal sealed class HostProtectionPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
    {
        private HostProtectionResource m_resources;
        internal static HostProtectionResource protectedResources;

        public HostProtectionPermission(HostProtectionResource resources)
        {
            this.Resources = resources;
        }

        public HostProtectionPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.Resources = HostProtectionResource.All;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.Resources = HostProtectionResource.None;
            }
        }

        public override IPermission Copy()
        {
            return new HostProtectionPermission(this.m_resources);
        }

        public override void FromXml(SecurityElement esd)
        {
            CodeAccessPermission.ValidateElement(esd, this);
            if (XMLUtil.IsUnrestricted(esd))
            {
                this.Resources = HostProtectionResource.All;
            }
            else
            {
                string str = esd.Attribute("Resources");
                if (str == null)
                {
                    this.Resources = HostProtectionResource.None;
                }
                else
                {
                    this.Resources = (HostProtectionResource) Enum.Parse(typeof(HostProtectionResource), str);
                }
            }
        }

        internal static int GetTokenIndex()
        {
            return 9;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            if (base.GetType() != target.GetType())
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            HostProtectionResource resources = this.m_resources & ((HostProtectionPermission) target).m_resources;
            if (resources == HostProtectionResource.None)
            {
                return null;
            }
            return new HostProtectionPermission(resources);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return (this.m_resources == HostProtectionResource.None);
            }
            if (base.GetType() != target.GetType())
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return ((this.m_resources & ((HostProtectionPermission) target).m_resources) == this.m_resources);
        }

        public bool IsUnrestricted()
        {
            return (this.Resources == HostProtectionResource.All);
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, base.GetType().FullName);
            if (this.IsUnrestricted())
            {
                element.AddAttribute("Unrestricted", "true");
                return element;
            }
            element.AddAttribute("Resources", XMLUtil.BitFieldEnumToString(typeof(HostProtectionResource), this.Resources));
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return this.Copy();
            }
            if (base.GetType() != target.GetType())
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return new HostProtectionPermission(this.m_resources | ((HostProtectionPermission) target).m_resources);
        }

        public HostProtectionResource Resources
        {
            get
            {
                return this.m_resources;
            }
            set
            {
                if ((value < HostProtectionResource.None) || (value > HostProtectionResource.All))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) value }));
                }
                this.m_resources = value;
            }
        }
    }
}

