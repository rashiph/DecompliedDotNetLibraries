namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public sealed class SqlClientPermission : DBDataPermission
    {
        [Obsolete("SqlClientPermission() has been deprecated.  Use the SqlClientPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
        public SqlClientPermission() : this(PermissionState.None)
        {
        }

        private SqlClientPermission(SqlClientPermission permission) : base(permission)
        {
        }

        internal SqlClientPermission(SqlClientPermissionAttribute permissionAttribute) : base(permissionAttribute)
        {
        }

        internal SqlClientPermission(SqlConnectionString constr) : base(constr)
        {
            if ((constr == null) || constr.IsEmpty)
            {
                base.Add(ADP.StrEmpty, ADP.StrEmpty, KeyRestrictionBehavior.AllowOnly);
            }
        }

        public SqlClientPermission(PermissionState state) : base(state)
        {
        }

        [Obsolete("SqlClientPermission(PermissionState state, Boolean allowBlankPassword) has been deprecated.  Use the SqlClientPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
        public SqlClientPermission(PermissionState state, bool allowBlankPassword) : this(state)
        {
            base.AllowBlankPassword = allowBlankPassword;
        }

        public override void Add(string connectionString, string restrictions, KeyRestrictionBehavior behavior)
        {
            DBConnectionString entry = new DBConnectionString(connectionString, restrictions, behavior, SqlConnectionString.GetParseSynonyms(), false);
            base.AddPermissionEntry(entry);
        }

        public override IPermission Copy()
        {
            return new SqlClientPermission(this);
        }
    }
}

