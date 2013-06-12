namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class DesigntimeLicenseContext : LicenseContext
    {
        internal Hashtable savedLicenseKeys = new Hashtable();

        public override string GetSavedLicenseKey(Type type, Assembly resourceAssembly)
        {
            return null;
        }

        public override void SetSavedLicenseKey(Type type, string key)
        {
            this.savedLicenseKeys[type.AssemblyQualifiedName] = key;
        }

        public override LicenseUsageMode UsageMode
        {
            get
            {
                return LicenseUsageMode.Designtime;
            }
        }
    }
}

