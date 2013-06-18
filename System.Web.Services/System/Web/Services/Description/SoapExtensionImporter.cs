namespace System.Web.Services.Description
{
    using System;
    using System.CodeDom;
    using System.Runtime;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public abstract class SoapExtensionImporter
    {
        private SoapProtocolImporter protocolImporter;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SoapExtensionImporter()
        {
        }

        public abstract void ImportMethod(CodeAttributeDeclarationCollection metadata);

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

