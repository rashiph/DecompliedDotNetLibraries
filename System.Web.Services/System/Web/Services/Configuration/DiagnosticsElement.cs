namespace System.Web.Services.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;

    public sealed class DiagnosticsElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty suppressReturningExceptions = new ConfigurationProperty("suppressReturningExceptions", typeof(bool), false);

        public DiagnosticsElement()
        {
            this.properties.Add(this.suppressReturningExceptions);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("suppressReturningExceptions", DefaultValue=false)]
        public bool SuppressReturningExceptions
        {
            get
            {
                return (bool) base[this.suppressReturningExceptions];
            }
            set
            {
                base[this.suppressReturningExceptions] = value;
            }
        }
    }
}

