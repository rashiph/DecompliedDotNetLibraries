namespace System.ComponentModel.Design
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class ComponentRenameEventArgs : EventArgs
    {
        private object component;
        private string newName;
        private string oldName;

        public ComponentRenameEventArgs(object component, string oldName, string newName)
        {
            this.oldName = oldName;
            this.newName = newName;
            this.component = component;
        }

        public object Component
        {
            get
            {
                return this.component;
            }
        }

        public virtual string NewName
        {
            get
            {
                return this.newName;
            }
        }

        public virtual string OldName
        {
            get
            {
                return this.oldName;
            }
        }
    }
}

