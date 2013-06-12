namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true), SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public abstract class BaseChannelSinkWithProperties : BaseChannelObjectWithProperties
    {
        protected BaseChannelSinkWithProperties()
        {
        }
    }
}

