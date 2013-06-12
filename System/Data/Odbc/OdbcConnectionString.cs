namespace System.Data.Odbc
{
    using System;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class OdbcConnectionString : DbConnectionOptions
    {
        private readonly string _expandedConnectionString;

        internal OdbcConnectionString(string connectionString, bool validate) : base(connectionString, null, true)
        {
            if (!validate)
            {
                string filename = null;
                int position = 0;
                this._expandedConnectionString = base.ExpandDataDirectories(ref filename, ref position);
            }
            if ((validate || (this._expandedConnectionString == null)) && ((connectionString != null) && (0x400 < connectionString.Length)))
            {
                throw ODBC.ConnectionStringTooLong();
            }
        }

        protected internal override PermissionSet CreatePermissionSet()
        {
            if (base.ContainsKey("savefile"))
            {
                return new NamedPermissionSet("FullTrust");
            }
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new OdbcPermission(this));
            return set;
        }

        protected internal override string Expand()
        {
            if (this._expandedConnectionString != null)
            {
                return this._expandedConnectionString;
            }
            return base.Expand();
        }
    }
}

