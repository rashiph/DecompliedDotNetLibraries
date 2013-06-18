namespace System.Configuration.Install
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    public abstract class ComponentInstaller : Installer
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ComponentInstaller()
        {
        }

        public abstract void CopyFromComponent(IComponent component);
        public virtual bool IsEquivalentInstaller(ComponentInstaller otherInstaller)
        {
            return false;
        }
    }
}

