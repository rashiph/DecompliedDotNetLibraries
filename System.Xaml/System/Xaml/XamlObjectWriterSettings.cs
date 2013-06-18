namespace System.Xaml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;
    using System.Xaml.Permissions;

    public class XamlObjectWriterSettings : XamlWriterSettings
    {
        public XamlObjectWriterSettings()
        {
        }

        public XamlObjectWriterSettings(XamlObjectWriterSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            this.AfterBeginInitHandler = settings.AfterBeginInitHandler;
            this.BeforePropertiesHandler = settings.BeforePropertiesHandler;
            this.AfterPropertiesHandler = settings.AfterPropertiesHandler;
            this.AfterEndInitHandler = settings.AfterEndInitHandler;
            this.XamlSetValueHandler = settings.XamlSetValueHandler;
            this.RootObjectInstance = settings.RootObjectInstance;
            this.IgnoreCanConvert = settings.IgnoreCanConvert;
            this.ExternalNameScope = settings.ExternalNameScope;
            this.SkipDuplicatePropertyCheck = settings.SkipDuplicatePropertyCheck;
            this.RegisterNamesOnExternalNamescope = settings.RegisterNamesOnExternalNamescope;
            this.AccessLevel = settings.AccessLevel;
            this.SkipProvideValueOnRoot = settings.SkipProvideValueOnRoot;
            this.PreferUnconvertedDictionaryKeys = settings.PreferUnconvertedDictionaryKeys;
        }

        internal XamlObjectWriterSettings StripDelegates()
        {
            return new XamlObjectWriterSettings(this) { AfterBeginInitHandler = null, AfterEndInitHandler = null, AfterPropertiesHandler = null, BeforePropertiesHandler = null, XamlSetValueHandler = null };
        }

        public XamlAccessLevel AccessLevel { get; set; }

        public EventHandler<XamlObjectEventArgs> AfterBeginInitHandler { get; set; }

        public EventHandler<XamlObjectEventArgs> AfterEndInitHandler { get; set; }

        public EventHandler<XamlObjectEventArgs> AfterPropertiesHandler { get; set; }

        public EventHandler<XamlObjectEventArgs> BeforePropertiesHandler { get; set; }

        public INameScope ExternalNameScope { get; set; }

        public bool IgnoreCanConvert { get; set; }

        public bool PreferUnconvertedDictionaryKeys { get; set; }

        public bool RegisterNamesOnExternalNamescope { get; set; }

        public object RootObjectInstance { get; set; }

        public bool SkipDuplicatePropertyCheck { get; set; }

        public bool SkipProvideValueOnRoot { get; set; }

        public EventHandler<XamlSetValueEventArgs> XamlSetValueHandler { get; set; }
    }
}

