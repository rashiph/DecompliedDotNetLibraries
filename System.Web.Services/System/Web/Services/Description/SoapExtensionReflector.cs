namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class SoapExtensionReflector
    {
        private ProtocolReflector protocolReflector;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SoapExtensionReflector()
        {
        }

        public virtual void ReflectDescription()
        {
        }

        public abstract void ReflectMethod();

        public ProtocolReflector ReflectionContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolReflector;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.protocolReflector = value;
            }
        }
    }
}

