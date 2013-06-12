namespace System.ComponentModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class EventDescriptor : MemberDescriptor
    {
        protected EventDescriptor(MemberDescriptor descr) : base(descr)
        {
        }

        protected EventDescriptor(MemberDescriptor descr, Attribute[] attrs) : base(descr, attrs)
        {
        }

        protected EventDescriptor(string name, Attribute[] attrs) : base(name, attrs)
        {
        }

        public abstract void AddEventHandler(object component, Delegate value);
        public abstract void RemoveEventHandler(object component, Delegate value);

        public abstract Type ComponentType { get; }

        public abstract Type EventType { get; }

        public abstract bool IsMulticast { get; }
    }
}

