namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class ComponentEditor
    {
        protected ComponentEditor()
        {
        }

        public bool EditComponent(object component)
        {
            return this.EditComponent(null, component);
        }

        public abstract bool EditComponent(ITypeDescriptorContext context, object component);
    }
}

