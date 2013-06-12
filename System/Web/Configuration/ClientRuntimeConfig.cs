namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Permissions;

    internal class ClientRuntimeConfig : RuntimeConfig
    {
        internal ClientRuntimeConfig() : base(null, false)
        {
        }

        [ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        protected override object GetSectionObject(string sectionName)
        {
            return ConfigurationManager.GetSection(sectionName);
        }
    }
}

