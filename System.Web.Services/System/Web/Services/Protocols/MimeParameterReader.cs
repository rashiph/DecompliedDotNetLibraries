namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Web;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class MimeParameterReader : MimeFormatter
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected MimeParameterReader()
        {
        }

        public abstract object[] Read(HttpRequest request);
    }
}

