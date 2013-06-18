namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class SoapTransportImporter
    {
        private SoapProtocolImporter protocolImporter;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SoapTransportImporter()
        {
        }

        public abstract void ImportClass();
        public abstract bool IsSupportedTransport(string transport);

        public SoapProtocolImporter ImportContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolImporter;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.protocolImporter = value;
            }
        }
    }
}

