namespace System.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public sealed class ZoneIdentityPermission : CodeAccessPermission, IBuiltInPermission
    {
        private const uint AllZones = 0x1f;
        [OptionalField(VersionAdded=2)]
        private string m_serializedPermission;
        private System.Security.SecurityZone m_zone;
        [OptionalField(VersionAdded=2)]
        private uint m_zones;

        public ZoneIdentityPermission(PermissionState state)
        {
            this.m_zone = System.Security.SecurityZone.NoZone;
            if (state == PermissionState.Unrestricted)
            {
                this.m_zones = 0x1f;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.m_zones = 0;
            }
        }

        public ZoneIdentityPermission(System.Security.SecurityZone zone)
        {
            this.m_zone = System.Security.SecurityZone.NoZone;
            this.SecurityZone = zone;
        }

        internal ZoneIdentityPermission(uint zones)
        {
            this.m_zone = System.Security.SecurityZone.NoZone;
            this.m_zones = zones & 0x1f;
        }

        internal void AppendZones(ArrayList zoneList)
        {
            int num = 0;
            for (uint i = 1; i < 0x1f; i = i << 1)
            {
                if ((this.m_zones & i) != 0)
                {
                    zoneList.Add((System.Security.SecurityZone) num);
                }
                num++;
            }
        }

        public override IPermission Copy()
        {
            return new ZoneIdentityPermission(this.m_zones);
        }

        public override void FromXml(SecurityElement esd)
        {
            this.m_zones = 0;
            CodeAccessPermission.ValidateElement(esd, this);
            string str = esd.Attribute("Zone");
            if (str != null)
            {
                this.SecurityZone = (System.Security.SecurityZone) Enum.Parse(typeof(System.Security.SecurityZone), str);
            }
            if (esd.Children != null)
            {
                foreach (SecurityElement element in esd.Children)
                {
                    str = element.Attribute("Zone");
                    int num = (int) Enum.Parse(typeof(System.Security.SecurityZone), str);
                    if (num != -1)
                    {
                        this.m_zones |= ((uint) 1) << num;
                    }
                }
            }
        }

        internal static int GetTokenIndex()
        {
            return 14;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            ZoneIdentityPermission permission = target as ZoneIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            uint zones = this.m_zones & permission.m_zones;
            if (zones == 0)
            {
                return null;
            }
            return new ZoneIdentityPermission(zones);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return (this.m_zones == 0);
            }
            ZoneIdentityPermission permission = target as ZoneIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return ((this.m_zones & permission.m_zones) == this.m_zones);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                if (this.m_serializedPermission != null)
                {
                    this.FromXml(SecurityElement.FromString(this.m_serializedPermission));
                    this.m_serializedPermission = null;
                }
                else
                {
                    this.SecurityZone = this.m_zone;
                    this.m_zone = System.Security.SecurityZone.NoZone;
                }
            }
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.m_serializedPermission = null;
                this.m_zone = System.Security.SecurityZone.NoZone;
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if ((ctx.State & ~(StreamingContextStates.CrossAppDomain | StreamingContextStates.Clone)) != 0)
            {
                this.m_serializedPermission = this.ToXml().ToString();
                this.m_zone = this.SecurityZone;
            }
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.ZoneIdentityPermission");
            if (this.SecurityZone != System.Security.SecurityZone.NoZone)
            {
                element.AddAttribute("Zone", Enum.GetName(typeof(System.Security.SecurityZone), this.SecurityZone));
                return element;
            }
            int num = 0;
            for (uint i = 1; i < 0x1f; i = i << 1)
            {
                if ((this.m_zones & i) != 0)
                {
                    SecurityElement child = new SecurityElement("Zone");
                    child.AddAttribute("Zone", Enum.GetName(typeof(System.Security.SecurityZone), (System.Security.SecurityZone) num));
                    element.AddChild(child);
                }
                num++;
            }
            return element;
        }

        public override IPermission Union(IPermission target)
        {
            if (target == null)
            {
                if (this.m_zones == 0)
                {
                    return null;
                }
                return this.Copy();
            }
            ZoneIdentityPermission permission = target as ZoneIdentityPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return new ZoneIdentityPermission(this.m_zones | permission.m_zones);
        }

        private static void VerifyZone(System.Security.SecurityZone zone)
        {
            if ((zone < System.Security.SecurityZone.NoZone) || (zone > System.Security.SecurityZone.Untrusted))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_IllegalZone"));
            }
        }

        public System.Security.SecurityZone SecurityZone
        {
            get
            {
                System.Security.SecurityZone noZone = System.Security.SecurityZone.NoZone;
                int num = 0;
                for (uint i = 1; i < 0x1f; i = i << 1)
                {
                    if ((this.m_zones & i) != 0)
                    {
                        if (noZone != System.Security.SecurityZone.NoZone)
                        {
                            return System.Security.SecurityZone.NoZone;
                        }
                        noZone = (System.Security.SecurityZone) num;
                    }
                    num++;
                }
                return noZone;
            }
            set
            {
                VerifyZone(value);
                if (value == System.Security.SecurityZone.NoZone)
                {
                    this.m_zones = 0;
                }
                else
                {
                    this.m_zones = ((uint) 1) << value;
                }
            }
        }
    }
}

