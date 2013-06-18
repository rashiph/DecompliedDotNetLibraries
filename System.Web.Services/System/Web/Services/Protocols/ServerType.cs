namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Security.Policy;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ServerType
    {
        private System.Type type;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ServerType(System.Type type)
        {
            this.type = type;
        }

        internal System.Security.Policy.Evidence Evidence
        {
            get
            {
                new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Assert();
                return this.Type.Assembly.Evidence;
            }
        }

        internal System.Type Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

