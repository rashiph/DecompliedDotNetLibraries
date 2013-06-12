namespace Microsoft.Win32
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class UserPreferenceChangingEventArgs : EventArgs
    {
        private readonly UserPreferenceCategory category;

        public UserPreferenceChangingEventArgs(UserPreferenceCategory category)
        {
            this.category = category;
        }

        public UserPreferenceCategory Category
        {
            get
            {
                return this.category;
            }
        }
    }
}

