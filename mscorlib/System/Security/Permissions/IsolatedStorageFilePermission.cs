namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true)]
    public sealed class IsolatedStorageFilePermission : IsolatedStoragePermission, IBuiltInPermission
    {
        public IsolatedStorageFilePermission(PermissionState state) : base(state)
        {
        }

        internal IsolatedStorageFilePermission(IsolatedStorageContainment UsageAllowed, long ExpirationDays, bool PermanentData) : base(UsageAllowed, ExpirationDays, PermanentData)
        {
        }

        public override IPermission Copy()
        {
            IsolatedStorageFilePermission permission = new IsolatedStorageFilePermission(PermissionState.Unrestricted);
            if (!base.IsUnrestricted())
            {
                permission.m_userQuota = base.m_userQuota;
                permission.m_machineQuota = base.m_machineQuota;
                permission.m_expirationDays = base.m_expirationDays;
                permission.m_permanentData = base.m_permanentData;
                permission.m_allowed = base.m_allowed;
            }
            return permission;
        }

        internal static int GetTokenIndex()
        {
            return 3;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            if (!base.VerifyType(target))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            IsolatedStorageFilePermission permission = (IsolatedStorageFilePermission) target;
            if (permission.IsUnrestricted())
            {
                return this.Copy();
            }
            if (base.IsUnrestricted())
            {
                return target.Copy();
            }
            IsolatedStorageFilePermission permission2 = new IsolatedStorageFilePermission(PermissionState.None) {
                m_userQuota = IsolatedStoragePermission.min(base.m_userQuota, permission.m_userQuota),
                m_machineQuota = IsolatedStoragePermission.min(base.m_machineQuota, permission.m_machineQuota),
                m_expirationDays = IsolatedStoragePermission.min(base.m_expirationDays, permission.m_expirationDays),
                m_permanentData = base.m_permanentData && permission.m_permanentData,
                m_allowed = (IsolatedStorageContainment) ((int) IsolatedStoragePermission.min((long) base.m_allowed, (long) permission.m_allowed))
            };
            if ((((permission2.m_userQuota == 0L) && (permission2.m_machineQuota == 0L)) && ((permission2.m_expirationDays == 0L) && !permission2.m_permanentData)) && (permission2.m_allowed == IsolatedStorageContainment.None))
            {
                return null;
            }
            return permission2;
        }

        public override bool IsSubsetOf(IPermission target)
        {
            bool flag;
            if (target == null)
            {
                return ((((base.m_userQuota == 0L) && (base.m_machineQuota == 0L)) && ((base.m_expirationDays == 0L) && !base.m_permanentData)) && (base.m_allowed == IsolatedStorageContainment.None));
            }
            try
            {
                IsolatedStorageFilePermission permission = (IsolatedStorageFilePermission) target;
                if (permission.IsUnrestricted())
                {
                    return true;
                }
                flag = ((((permission.m_userQuota >= base.m_userQuota) && (permission.m_machineQuota >= base.m_machineQuota)) && (permission.m_expirationDays >= base.m_expirationDays)) && (permission.m_permanentData || !base.m_permanentData)) && (permission.m_allowed >= base.m_allowed);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return flag;
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        [ComVisible(false)]
        public override SecurityElement ToXml()
        {
            return base.ToXml("System.Security.Permissions.IsolatedStorageFilePermission");
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
            IsolatedStorageFilePermission permission = (IsolatedStorageFilePermission) target;
            if (base.IsUnrestricted() || permission.IsUnrestricted())
            {
                return new IsolatedStorageFilePermission(PermissionState.Unrestricted);
            }
            return new IsolatedStorageFilePermission(PermissionState.None) { m_userQuota = IsolatedStoragePermission.max(base.m_userQuota, permission.m_userQuota), m_machineQuota = IsolatedStoragePermission.max(base.m_machineQuota, permission.m_machineQuota), m_expirationDays = IsolatedStoragePermission.max(base.m_expirationDays, permission.m_expirationDays), m_permanentData = base.m_permanentData || permission.m_permanentData, m_allowed = (IsolatedStorageContainment) ((int) IsolatedStoragePermission.max((long) base.m_allowed, (long) permission.m_allowed)) };
        }
    }
}

