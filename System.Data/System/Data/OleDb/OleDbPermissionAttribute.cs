namespace System.Data.OleDb
{
    using System;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public sealed class OleDbPermissionAttribute : DBDataPermissionAttribute
    {
        private string _providers;

        public OleDbPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            return new OleDbPermission(this);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Obsolete("Provider property has been deprecated.  Use the Add method.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public string Provider
        {
            get
            {
                string str = this._providers;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                this._providers = value;
            }
        }
    }
}

