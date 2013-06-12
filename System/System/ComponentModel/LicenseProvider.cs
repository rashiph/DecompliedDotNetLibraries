namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class LicenseProvider
    {
        protected LicenseProvider()
        {
        }

        public abstract License GetLicense(LicenseContext context, Type type, object instance, bool allowExceptions);
    }
}

