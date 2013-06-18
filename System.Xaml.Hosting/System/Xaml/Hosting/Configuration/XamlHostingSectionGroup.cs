namespace System.Xaml.Hosting.Configuration
{
    using System.Configuration;

    public sealed class XamlHostingSectionGroup : ConfigurationSectionGroup
    {
        public System.Xaml.Hosting.Configuration.XamlHostingSection XamlHostingSection
        {
            get
            {
                return (System.Xaml.Hosting.Configuration.XamlHostingSection) base.Sections["system.xaml.hosting"];
            }
        }
    }
}

