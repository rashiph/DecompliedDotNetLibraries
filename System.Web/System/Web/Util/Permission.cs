namespace System.Web.Util
{
    using System;
    using System.Data.SqlClient;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;

    internal static class Permission
    {
        internal static bool HasSqlClientPermission()
        {
            NamedPermissionSet namedPermissionSet = HttpRuntime.NamedPermissionSet;
            if (namedPermissionSet == null)
            {
                return true;
            }
            IPermission target = namedPermissionSet.GetPermission(typeof(SqlClientPermission));
            if (target == null)
            {
                return false;
            }
            IPermission permission2 = null;
            try
            {
                permission2 = new SqlClientPermission(PermissionState.Unrestricted);
            }
            catch
            {
                return false;
            }
            return permission2.IsSubsetOf(target);
        }
    }
}

