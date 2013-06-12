namespace System.Data.OleDb
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public sealed class OleDbPermission : DBDataPermission
    {
        private string[] _providerRestriction;
        private string _providers;

        [Obsolete("OleDbPermission() has been deprecated.  Use the OleDbPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
        public OleDbPermission() : this(PermissionState.None)
        {
        }

        internal OleDbPermission(OleDbConnectionString constr) : base(constr)
        {
            if ((constr == null) || constr.IsEmpty)
            {
                base.Add(ADP.StrEmpty, ADP.StrEmpty, KeyRestrictionBehavior.AllowOnly);
            }
        }

        private OleDbPermission(OleDbPermission permission) : base(permission)
        {
        }

        internal OleDbPermission(OleDbPermissionAttribute permissionAttribute) : base(permissionAttribute)
        {
        }

        public OleDbPermission(PermissionState state) : base(state)
        {
        }

        [Obsolete("OleDbPermission(PermissionState state, Boolean allowBlankPassword) has been deprecated.  Use the OleDbPermission(PermissionState.None) constructor.  http://go.microsoft.com/fwlink/?linkid=14202", true)]
        public OleDbPermission(PermissionState state, bool allowBlankPassword) : this(state)
        {
            base.AllowBlankPassword = allowBlankPassword;
        }

        public override IPermission Copy()
        {
            return new OleDbPermission(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Obsolete("Provider property has been deprecated.  Use the Add method.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public string Provider
        {
            get
            {
                string str = this._providers;
                if (str == null)
                {
                    string[] strArray = this._providerRestriction;
                    if ((strArray != null) && (0 < strArray.Length))
                    {
                        str = strArray[0];
                        for (int i = 1; i < strArray.Length; i++)
                        {
                            str = str + ";" + strArray[i];
                        }
                    }
                }
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                string[] strArray = null;
                if (!ADP.IsEmpty(value))
                {
                    strArray = DBConnectionString.RemoveDuplicates(value.Split(new char[] { ';' }));
                }
                this._providerRestriction = strArray;
                this._providers = value;
            }
        }
    }
}

