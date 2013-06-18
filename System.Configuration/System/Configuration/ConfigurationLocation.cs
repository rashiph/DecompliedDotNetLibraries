namespace System.Configuration
{
    using System;
    using System.Runtime;

    public class ConfigurationLocation
    {
        private System.Configuration.Configuration _config;
        private string _locationSubPath;

        internal ConfigurationLocation(System.Configuration.Configuration config, string locationSubPath)
        {
            this._config = config;
            this._locationSubPath = locationSubPath;
        }

        public System.Configuration.Configuration OpenConfiguration()
        {
            return this._config.OpenLocationConfiguration(this._locationSubPath);
        }

        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._locationSubPath;
            }
        }
    }
}

