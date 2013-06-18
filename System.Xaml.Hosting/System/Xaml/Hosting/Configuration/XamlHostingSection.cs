namespace System.Xaml.Hosting.Configuration
{
    using System.Configuration;

    public sealed class XamlHostingSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection=true)]
        public HandlerElementCollection Handlers
        {
            get
            {
                return (HandlerElementCollection) base[""];
            }
        }
    }
}

