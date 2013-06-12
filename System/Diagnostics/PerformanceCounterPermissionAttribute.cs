namespace System.Diagnostics
{
    using System;
    using System.ComponentModel;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public class PerformanceCounterPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string categoryName;
        private string machineName;
        private PerformanceCounterPermissionAccess permissionAccess;

        public PerformanceCounterPermissionAttribute(SecurityAction action) : base(action)
        {
            this.categoryName = "*";
            this.machineName = ".";
            this.permissionAccess = PerformanceCounterPermissionAccess.Write;
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new PerformanceCounterPermission(PermissionState.Unrestricted);
            }
            return new PerformanceCounterPermission(this.PermissionAccess, this.MachineName, this.CategoryName);
        }

        public string CategoryName
        {
            get
            {
                return this.categoryName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.categoryName = value;
            }
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

        public PerformanceCounterPermissionAccess PermissionAccess
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

