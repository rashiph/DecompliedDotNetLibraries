namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public sealed class OdbcPermission : DBDataPermission
    {
        [Obsolete("OdbcPermission() has been deprecated.  Use the OdbcPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
        public OdbcPermission() : this(PermissionState.None)
        {
        }

        internal OdbcPermission(OdbcConnectionString constr) : base(constr)
        {
            if ((constr == null) || constr.IsEmpty)
            {
                base.Add(ADP.StrEmpty, ADP.StrEmpty, KeyRestrictionBehavior.AllowOnly);
            }
        }

        private OdbcPermission(OdbcPermission permission) : base(permission)
        {
        }

        internal OdbcPermission(OdbcPermissionAttribute permissionAttribute) : base(permissionAttribute)
        {
        }

        public OdbcPermission(PermissionState state) : base(state)
        {
        }

        [Obsolete("OdbcPermission(PermissionState state, Boolean allowBlankPassword) has been deprecated.  Use the OdbcPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
        public OdbcPermission(PermissionState state, bool allowBlankPassword) : this(state)
        {
            base.AllowBlankPassword = allowBlankPassword;
        }

        public override void Add(string connectionString, string restrictions, KeyRestrictionBehavior behavior)
        {
            DBConnectionString entry = new DBConnectionString(connectionString, restrictions, behavior, null, true);
            base.AddPermissionEntry(entry);
        }

        public override IPermission Copy()
        {
            return new OdbcPermission(this);
        }
    }
}

