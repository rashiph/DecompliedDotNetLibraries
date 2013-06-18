namespace System.Drawing.Design
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ToolboxComponentsCreatedEventArgs : EventArgs
    {
        private readonly IComponent[] comps;

        public ToolboxComponentsCreatedEventArgs(IComponent[] components)
        {
            this.comps = components;
        }

        public IComponent[] Components
        {
            get
            {
                return (IComponent[]) this.comps.Clone();
            }
        }
    }
}

