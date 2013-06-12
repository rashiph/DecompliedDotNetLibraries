namespace System.Data.Common
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public abstract class DBDataPermissionAttribute : CodeAccessSecurityAttribute
    {
        private bool _allowBlankPassword;
        private System.Data.KeyRestrictionBehavior _behavior;
        private string _connectionString;
        private string _restrictions;

        protected DBDataPermissionAttribute(SecurityAction action) : base(action)
        {
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
                throw ADP.InvalidKeyRestrictionBehavior(value);
            }
        }

        public string KeyRestrictions
        {
            get
            {
                string str = this._restrictions;
                if (str == null)
                {
                    return ADP.StrEmpty;
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

