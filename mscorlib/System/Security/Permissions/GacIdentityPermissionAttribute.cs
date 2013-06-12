namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple=true, Inherited=false), ComVisible(true)]
    public sealed class GacIdentityPermissionAttribute : CodeAccessSecurityAttribute
    {
        public GacIdentityPermissionAttribute(SecurityAction action) : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            return new GacIdentityPermission();
        }
    }
}

