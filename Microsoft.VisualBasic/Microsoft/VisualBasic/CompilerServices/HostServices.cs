namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Security.Permissions;

    [EditorBrowsable(EditorBrowsableState.Never), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.SharedState)]
    public sealed class HostServices
    {
        private static IVbHost m_host;

        public static IVbHost VBHost
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return m_host;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                m_host = value;
            }
        }
    }
}

