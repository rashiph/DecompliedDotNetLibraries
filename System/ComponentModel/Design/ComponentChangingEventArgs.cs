namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public sealed class ComponentChangingEventArgs : EventArgs
    {
        private object component;
        private MemberDescriptor member;

        public ComponentChangingEventArgs(object component, MemberDescriptor member)
        {
            this.component = component;
            this.member = member;
        }

        public object Component
        {
            get
            {
                return this.component;
            }
        }

        public MemberDescriptor Member
        {
            get
            {
                return this.member;
            }
        }
    }
}

