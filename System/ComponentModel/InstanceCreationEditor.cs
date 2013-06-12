namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class InstanceCreationEditor
    {
        protected InstanceCreationEditor()
        {
        }

        public abstract object CreateInstance(ITypeDescriptorContext context, Type instanceType);

        public virtual string Text
        {
            get
            {
                return SR.GetString("InstanceCreationEditorDefaultText");
            }
        }
    }
}

