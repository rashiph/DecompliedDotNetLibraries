namespace System.Diagnostics
{
    using System;
    using System.ComponentModel;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public class EventLogPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string machineName;
        private EventLogPermissionAccess permissionAccess;

        public EventLogPermissionAttribute(SecurityAction action) : base(action)
        {
            this.machineName = ".";
            this.permissionAccess = EventLogPermissionAccess.Write;
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new EventLogPermission(PermissionState.Unrestricted);
            }
            return new EventLogPermission(this.PermissionAccess, this.MachineName);
        }

        public string MachineName
        {
            get
            {
                return this.machineName;
            }
            set
            {
                if (!SyntaxCheck.CheckMachineName(value))
                {
                    throw new ArgumentException(SR.GetString("InvalidProperty", new object[] { "MachineName", value }));
                }
                this.machineName = value;
            }
        }

        public EventLogPermissionAccess PermissionAccess
        {
            get
            {
                return this.permissionAccess;
            }
            set
            {
                this.permissionAccess = value;
            }
        }
    }
}

