namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ResolveNameEventArgs : EventArgs
    {
        private string name;
        private object value;

        public ResolveNameEventArgs(string name)
        {
            this.name = name;
            this.value = null;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

