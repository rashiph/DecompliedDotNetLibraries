namespace System.Security.Permissions
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable, ComVisible(true), SecurityPermission(SecurityAction.InheritanceDemand, ControlEvidence=true, ControlPolicy=true)]
    public abstract class IsolatedStoragePermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private const string _strExpiry = "Expiry";
        private const string _strMachineQuota = "MachineQuota";
        private const string _strPermDat = "Permanent";
        private const string _strUserQuota = "UserQuota";
        internal IsolatedStorageContainment m_allowed;
        internal long m_expirationDays;
        internal long m_machineQuota;
        internal bool m_permanentData;
        internal long m_userQuota;

        protected IsolatedStoragePermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.m_userQuota = 0x7fffffffffffffffL;
                this.m_machineQuota = 0x7fffffffffffffffL;
                this.m_expirationDays = 0x7fffffffffffffffL;
                this.m_permanentData = true;
                this.m_allowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.m_userQuota = 0L;
                this.m_machineQuota = 0L;
                this.m_expirationDays = 0L;
                this.m_permanentData = false;
                this.m_allowed = IsolatedStorageContainment.None;
            }
        }

        internal IsolatedStoragePermission(IsolatedStorageContainment UsageAllowed, long ExpirationDays, bool PermanentData)
        {
            this.m_userQuota = 0L;
            this.m_machineQuota = 0L;
            this.m_expirationDays = ExpirationDays;
            this.m_permanentData = PermanentData;
            this.m_allowed = UsageAllowed;
        }

        internal IsolatedStoragePermission(IsolatedStorageContainment UsageAllowed, long ExpirationDays, bool PermanentData, long UserQuota)
        {
            this.m_machineQuota = 0L;
            this.m_userQuota = UserQuota;
            this.m_expirationDays = ExpirationDays;
            this.m_permanentData = PermanentData;
            this.m_allowed = UsageAllowed;
        }

        public override void FromXml(SecurityElement esd)
        {
            CodeAccessPermission.ValidateElement(esd, this);
            this.m_allowed = IsolatedStorageContainment.None;
            if (XMLUtil.IsUnrestricted(esd))
            {
                this.m_allowed = IsolatedStorageContainment.UnrestrictedIsolatedStorage;
            }
            else
            {
                string str = esd.Attribute("Allowed");
                if (str != null)
                {
                    this.m_allowed = (IsolatedStorageContainment) Enum.Parse(typeof(IsolatedStorageContainment), str);
                }
            }
            if (this.m_allowed == IsolatedStorageContainment.UnrestrictedIsolatedStorage)
            {
                this.m_userQuota = 0x7fffffffffffffffL;
                this.m_machineQuota = 0x7fffffffffffffffL;
                this.m_expirationDays = 0x7fffffffffffffffL;
                this.m_permanentData = true;
            }
            else
            {
                string s = esd.Attribute("UserQuota");
                this.m_userQuota = (s != null) ? long.Parse(s, CultureInfo.InvariantCulture) : 0L;
                s = esd.Attribute("MachineQuota");
                this.m_machineQuota = (s != null) ? long.Parse(s, CultureInfo.InvariantCulture) : 0L;
                s = esd.Attribute("Expiry");
                this.m_expirationDays = (s != null) ? long.Parse(s, CultureInfo.InvariantCulture) : 0L;
                s = esd.Attribute("Permanent");
                this.m_permanentData = (s != null) ? bool.Parse(s) : false;
            }
        }

        public bool IsUnrestricted()
        {
            return (this.m_allowed == IsolatedStorageContainment.UnrestrictedIsolatedStorage);
        }

        internal static long max(long x, long y)
        {
            if (x >= y)
            {
                return x;
            }
            return y;
        }

        internal static long min(long x, long y)
        {
            if (x <= y)
            {
                return x;
            }
            return y;
        }

        public override SecurityElement ToXml()
        {
            return this.ToXml(base.GetType().FullName);
        }

        internal SecurityElement ToXml(string permName)
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, permName);
            if (!this.IsUnrestricted())
            {
                element.AddAttribute("Allowed", Enum.GetName(typeof(IsolatedStorageContainment), this.m_allowed));
                if (this.m_userQuota > 0L)
                {
                    element.AddAttribute("UserQuota", this.m_userQuota.ToString(CultureInfo.InvariantCulture));
                }
                if (this.m_machineQuota > 0L)
                {
                    element.AddAttribute("MachineQuota", this.m_machineQuota.ToString(CultureInfo.InvariantCulture));
                }
                if (this.m_expirationDays > 0L)
                {
                    element.AddAttribute("Expiry", this.m_expirationDays.ToString(CultureInfo.InvariantCulture));
                }
                if (this.m_permanentData)
                {
                    element.AddAttribute("Permanent", this.m_permanentData.ToString());
                }
                return element;
            }
            element.AddAttribute("Unrestricted", "true");
            return element;
        }

        public IsolatedStorageContainment UsageAllowed
        {
            get
            {
                return this.m_allowed;
            }
            set
            {
                this.m_allowed = value;
            }
        }

        public long UserQuota
        {
            get
            {
                return this.m_userQuota;
            }
            set
            {
                this.m_userQuota = value;
            }
        }
    }
}

