namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class ContainerFilterService
    {
        protected ContainerFilterService()
        {
        }

        public virtual ComponentCollection FilterComponents(ComponentCollection components)
        {
            return components;
        }
    }
}

