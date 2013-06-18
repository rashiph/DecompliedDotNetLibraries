namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.ServiceModel.Description;

    public class StandardEndpointCollectionElement<TStandardEndpoint, TEndpointConfiguration> : EndpointCollectionElement where TStandardEndpoint: ServiceEndpoint where TEndpointConfiguration: StandardEndpointElement, new()
    {
        private ConfigurationPropertyCollection properties;

        public override bool ContainsKey(string name)
        {
            return element.Endpoints.ContainsKey(name);
        }

        protected internal override StandardEndpointElement GetDefaultStandardEndpointElement()
        {
            return Activator.CreateInstance<TEndpointConfiguration>();
        }

        protected internal override bool TryAdd(string name, ServiceEndpoint endpoint, System.Configuration.Configuration config)
        {
            bool flag = (endpoint.GetType() == typeof(TStandardEndpoint)) && typeof(StandardEndpointElement).IsAssignableFrom(typeof(TEndpointConfiguration));
            if (flag)
            {
                TEndpointConfiguration element = Activator.CreateInstance<TEndpointConfiguration>();
                element.Name = name;
                element.InitializeFrom(endpoint);
                this.Endpoints.Add(element);
            }
            return flag;
        }

        public override ReadOnlyCollection<StandardEndpointElement> ConfiguredEndpoints
        {
            get
            {
                List<StandardEndpointElement> list = new List<StandardEndpointElement>();
                foreach (StandardEndpointElement element in this.Endpoints)
                {
                    list.Add(element);
                }
                return new ReadOnlyCollection<StandardEndpointElement>(list);
            }
        }

        [ConfigurationProperty("", Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public StandardEndpointElementCollection<TEndpointConfiguration> Endpoints
        {
            get
            {
                return (StandardEndpointElementCollection<TEndpointConfiguration>) base[""];
            }
        }

        public override Type EndpointType
        {
            get
            {
                return typeof(TStandardEndpoint);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("", typeof(StandardEndpointElementCollection<TEndpointConfiguration>), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

