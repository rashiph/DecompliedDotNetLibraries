namespace System.Configuration
{
    using System;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, AttributeUsage(AttributeTargets.All, AllowMultiple=true, Inherited=false)]
    public sealed class ConfigurationPermissionAttribute : CodeAccessSecurityAttribute
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConfigurationPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            return new ConfigurationPermission(base.Unrestricted ? PermissionState.Unrestricted : PermissionState.None);
        }
    }
}

