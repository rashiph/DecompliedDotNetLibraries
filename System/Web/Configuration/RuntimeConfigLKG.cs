namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.Security.Permissions;

    internal class RuntimeConfigLKG : RuntimeConfig
    {
        internal RuntimeConfigLKG(IInternalConfigRecord configRecord) : base(configRecord, true)
        {
        }

        [ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        protected override object GetSectionObject(string sectionName)
        {
            if (base._configRecord != null)
            {
                return base._configRecord.GetLkgSection(sectionName);
            }
            try
            {
                return ConfigurationManager.GetSection(sectionName);
            }
            catch
            {
                return null;
            }
        }
    }
}

