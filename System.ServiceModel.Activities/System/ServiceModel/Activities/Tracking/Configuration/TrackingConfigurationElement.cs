namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;

    public abstract class TrackingConfigurationElement : ConfigurationElement
    {
        protected TrackingConfigurationElement()
        {
        }

        protected static string GetStringPairKey(string value1, string value2)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}{2}", new object[] { (value1 == null) ? 0 : value1.Length, value1, value2 });
        }

        public abstract object ElementKey { get; }
    }
}

