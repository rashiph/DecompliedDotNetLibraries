namespace System.Data.Odbc
{
    using System;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class OdbcPermissionAttribute : DBDataPermissionAttribute
    {
        public OdbcPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            return new OdbcPermission(this);
        }
    }
}

