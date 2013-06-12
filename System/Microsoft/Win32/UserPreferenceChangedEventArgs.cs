namespace Microsoft.Win32
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class UserPreferenceChangedEventArgs : EventArgs
    {
        private readonly UserPreferenceCategory category;

        public UserPreferenceChangedEventArgs(UserPreferenceCategory category)
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

