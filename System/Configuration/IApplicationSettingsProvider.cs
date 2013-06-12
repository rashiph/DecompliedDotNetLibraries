namespace System.Configuration
{
    using System;
    using System.Security.Permissions;

    public interface IApplicationSettingsProvider
    {
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Reset(SettingsContext context);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Upgrade(SettingsContext context, SettingsPropertyCollection properties);
    }
}

