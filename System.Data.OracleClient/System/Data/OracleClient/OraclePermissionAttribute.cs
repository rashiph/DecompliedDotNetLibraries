namespace System.Data.OracleClient
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, Obsolete("OraclePermissionAttribute has been deprecated. http://go.microsoft.com/fwlink/?LinkID=144260", false), AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class OraclePermissionAttribute : CodeAccessSecurityAttribute
    {
        private bool _allowBlankPassword;
        private System.Data.KeyRestrictionBehavior _behavior;
        private string _connectionString;
        private string _restrictions;

        public OraclePermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            return new OraclePermission(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeConnectionString()
        {
            return (null != this._connectionString);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeKeyRestrictions()
        {
            return (null != this._restrictions);
        }

        public bool AllowBlankPassword
        {
            get
            {
                return this._allowBlankPassword;
            }
            set
            {
                this._allowBlankPassword = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                string str = this._connectionString;
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this._connectionString = value;
            }
        }

        public System.Data.KeyRestrictionBehavior KeyRestrictionBehavior
        {
            get
            {
                return this._behavior;
            }
            set
            {
                switch (value)
                {
                    case System.Data.KeyRestrictionBehavior.AllowOnly:
                    case System.Data.KeyRestrictionBehavior.PreventUsage:
                        this._behavior = value;
                        return;
                }
                throw System.Data.Common.ADP.InvalidKeyRestrictionBehavior(value);
            }
        }

        public string KeyRestrictions
        {
            get
            {
                string str = this._restrictions;
                if (str == null)
                {
                    return System.Data.Common.ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                this._restrictions = value;
            }
        }
    }
}

