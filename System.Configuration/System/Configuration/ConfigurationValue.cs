namespace System.Configuration
{
    using System;

    internal class ConfigurationValue
    {
        internal PropertySourceInfo SourceInfo;
        internal object Value;
        internal ConfigurationValueFlags ValueFlags;

        internal ConfigurationValue(object value, ConfigurationValueFlags valueFlags, PropertySourceInfo sourceInfo)
        {
            this.Value = value;
            this.ValueFlags = valueFlags;
            this.SourceInfo = sourceInfo;
        }
    }
}

