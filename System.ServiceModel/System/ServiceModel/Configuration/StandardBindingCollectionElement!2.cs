namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.ServiceModel.Channels;

    public class StandardBindingCollectionElement<TStandardBinding, TBindingConfiguration> : BindingCollectionElement where TStandardBinding: Binding where TBindingConfiguration: StandardBindingElement, new()
    {
        private ConfigurationPropertyCollection properties;

        public override bool ContainsKey(string name)
        {
            return element.Bindings.ContainsKey(name);
        }

        protected internal override Binding GetDefault()
        {
            return Activator.CreateInstance<TStandardBinding>();
        }

        protected internal override bool TryAdd(string name, Binding binding, System.Configuration.Configuration config)
        {
            bool flag = (binding.GetType() == typeof(TStandardBinding)) && typeof(StandardBindingElement).IsAssignableFrom(typeof(TBindingConfiguration));
            if (flag)
            {
                TBindingConfiguration element = Activator.CreateInstance<TBindingConfiguration>();
                element.Name = name;
                element.InitializeFrom(binding);
                this.Bindings.Add(element);
            }
            return flag;
        }

        [ConfigurationProperty("", Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public StandardBindingElementCollection<TBindingConfiguration> Bindings
        {
            get
            {
                return (StandardBindingElementCollection<TBindingConfiguration>) base[""];
            }
        }

        public override System.Type BindingType
        {
            get
            {
                return typeof(TStandardBinding);
            }
        }

        public override ReadOnlyCollection<IBindingConfigurationElement> ConfiguredBindings
        {
            get
            {
                List<IBindingConfigurationElement> list = new List<IBindingConfigurationElement>();
                foreach (IBindingConfigurationElement element in this.Bindings)
                {
                    list.Add(element);
                }
                return new ReadOnlyCollection<IBindingConfigurationElement>(list);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("", typeof(StandardBindingElementCollection<TBindingConfiguration>), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

