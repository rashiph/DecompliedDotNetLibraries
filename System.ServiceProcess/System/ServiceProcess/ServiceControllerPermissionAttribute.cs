namespace System.ServiceProcess
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false)]
    public class ServiceControllerPermissionAttribute : CodeAccessSecurityAttribute
    {
        private string machineName;
        private ServiceControllerPermissionAccess permissionAccess;
        private string serviceName;

        public ServiceControllerPermissionAttribute(SecurityAction action) : base(action)
        {
            this.machineName = ".";
            this.serviceName = "*";
            this.permissionAccess = ServiceControllerPermissionAccess.Browse;
        }

        public override IPermission CreatePermission()
        {
            if (base.Unrestricted)
            {
                return new ServiceControllerPermission(PermissionState.Unrestricted);
            }
            return new ServiceControllerPermission(this.PermissionAccess, this.MachineName, this.ServiceName);
        }

        public string MachineName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.machineName;
            }
            set
            {
                if (!SyntaxCheck.CheckMachineName(value))
                {
                    throw new ArgumentException(Res.GetString("BadMachineName", new object[] { value }));
                }
                this.machineName = value;
            }
        }

        public ServiceControllerPermissionAccess PermissionAccess
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.permissionAccess;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.permissionAccess = value;
            }
        }

        public string ServiceName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serviceName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!ServiceController.ValidServiceName(value))
                {
                    object[] args = new object[] { value, 80.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(Res.GetString("ServiceName", args));
                }
                this.serviceName = value;
            }
        }
    }
}

