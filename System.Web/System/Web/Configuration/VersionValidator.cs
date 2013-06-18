namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;

    internal sealed class VersionValidator : ConfigurationValidatorBase
    {
        private readonly Version _minimumVersion;

        public VersionValidator(Version minimumVersion)
        {
            this._minimumVersion = minimumVersion;
        }

        public override bool CanValidate(Type type)
        {
            return typeof(Version).Equals(type);
        }

        public override void Validate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (((Version) value) < this._minimumVersion)
            {
                throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("Config_control_rendering_compatibility_version_is_less_than_minimum_version"));
            }
        }
    }
}

