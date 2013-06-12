namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Util;

    public sealed class WebConfigurationFileMap : ConfigurationFileMap
    {
        private string _site;
        private VirtualDirectoryMappingCollection _virtualDirectoryMapping;

        public WebConfigurationFileMap()
        {
            this._site = string.Empty;
            this._virtualDirectoryMapping = new VirtualDirectoryMappingCollection();
        }

        public WebConfigurationFileMap(string machineConfigFileName) : base(machineConfigFileName)
        {
            this._site = string.Empty;
            this._virtualDirectoryMapping = new VirtualDirectoryMappingCollection();
        }

        private WebConfigurationFileMap(string machineConfigFileName, string site, VirtualDirectoryMappingCollection VirtualDirectoryMapping) : base(machineConfigFileName)
        {
            this._site = site;
            this._virtualDirectoryMapping = VirtualDirectoryMapping;
        }

        public override object Clone()
        {
            return new WebConfigurationFileMap(base.MachineConfigFilename, this._site, this._virtualDirectoryMapping.Clone());
        }

        internal string Site
        {
            get
            {
                return this._site;
            }
            set
            {
                if (!WebConfigurationHost.IsValidSiteArgument(value))
                {
                    throw System.Web.Util.ExceptionUtil.PropertyInvalid("Site");
                }
                this._site = value;
            }
        }

        public VirtualDirectoryMappingCollection VirtualDirectories
        {
            get
            {
                return this._virtualDirectoryMapping;
            }
        }
    }
}

