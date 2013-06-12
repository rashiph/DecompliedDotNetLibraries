namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ComponentEventArgs : EventArgs
    {
        private IComponent component;

        public ComponentEventArgs(IComponent component)
        {
            this.component = component;
        }

        public virtual IComponent Component
        {
            get
            {
                return this.component;
            }
        }
    }
}

