namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class SqlClientPermissionAttribute : DBDataPermissionAttribute
    {
        public SqlClientPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            return new SqlClientPermission(this);
        }
    }
}

