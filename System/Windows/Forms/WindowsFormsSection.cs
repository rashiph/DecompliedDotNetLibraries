namespace System.Windows.Forms
{
    using System;
    using System.Configuration;

    public sealed class WindowsFormsSection : ConfigurationSection
    {
        internal const bool JitDebuggingDefault = false;
        private static ConfigurationPropertyCollection s_properties;
        private static ConfigurationProperty s_propJitDebugging;

        public WindowsFormsSection()
        {
            EnsureStaticPropertyBag();
        }

        private static ConfigurationPropertyCollection EnsureStaticPropertyBag()
        {
            if (s_properties == null)
            {
                s_propJitDebugging = new ConfigurationProperty("jitDebugging", typeof(bool), false, ConfigurationPropertyOptions.None);
                ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                propertys.Add(s_propJitDebugging);
                s_properties = propertys;
            }
            return s_properties;
        }

        internal static WindowsFormsSection GetSection()
        {
            try
            {
                return (WindowsFormsSection) System.Configuration.PrivilegedConfigurationManager.GetSection("system.windows.forms");
            }
            catch
            {
                return new WindowsFormsSection();
            }
        }

        [ConfigurationProperty("jitDebugging", DefaultValue=false)]
        public bool JitDebugging
        {
            get
            {
                return (bool) base[s_propJitDebugging];
            }
            set
            {
                base[s_propJitDebugging] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return EnsureStaticPropertyBag();
            }
        }
    }
}

