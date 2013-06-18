namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    public sealed class ProviderSettings : ConfigurationElement
    {
        private ConfigurationPropertyCollection _properties;
        private NameValueCollection _PropertyNameCollection;
        private readonly ConfigurationProperty _propName;
        private readonly ConfigurationProperty _propType;

        public ProviderSettings()
        {
            this._propName = new ConfigurationProperty("name", typeof(string), null, null, ConfigurationProperty.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
            this._propType = new ConfigurationProperty("type", typeof(string), "", ConfigurationPropertyOptions.IsTypeStringTransformationRequired | ConfigurationPropertyOptions.IsRequired);
            this._properties = new ConfigurationPropertyCollection();
            this._properties.Add(this._propName);
            this._properties.Add(this._propType);
            this._PropertyNameCollection = null;
        }

        public ProviderSettings(string name, string type) : this()
        {
            this.Name = name;
            this.Type = type;
        }

        private string GetProperty(string PropName)
        {
            if (this._properties.Contains(PropName))
            {
                ConfigurationProperty property = this._properties[PropName];
                if (property != null)
                {
                    return (string) base[property];
                }
            }
            return null;
        }

        protected internal override bool IsModified()
        {
            if (!this.UpdatePropertyCollection())
            {
                return base.IsModified();
            }
            return true;
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            ConfigurationProperty property = new ConfigurationProperty(name, typeof(string), value);
            this._properties.Add(property);
            base[property] = value;
            this.Parameters[name] = value;
            return true;
        }

        protected internal override void Reset(ConfigurationElement parentElement)
        {
            ProviderSettings settings = parentElement as ProviderSettings;
            if (settings != null)
            {
                settings.UpdatePropertyCollection();
            }
            base.Reset(parentElement);
        }

        private bool SetProperty(string PropName, string value)
        {
            ConfigurationProperty property = null;
            if (this._properties.Contains(PropName))
            {
                property = this._properties[PropName];
            }
            else
            {
                property = new ConfigurationProperty(PropName, typeof(string), null);
                this._properties.Add(property);
            }
            if (property != null)
            {
                base[property] = value;
                return true;
            }
            return false;
        }

        protected internal override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            ProviderSettings settings = parentElement as ProviderSettings;
            if (settings != null)
            {
                settings.UpdatePropertyCollection();
            }
            ProviderSettings settings2 = sourceElement as ProviderSettings;
            if (settings2 != null)
            {
                settings2.UpdatePropertyCollection();
            }
            base.Unmerge(sourceElement, parentElement, saveMode);
            this.UpdatePropertyCollection();
        }

        internal bool UpdatePropertyCollection()
        {
            bool flag = false;
            ArrayList list = null;
            if (this._PropertyNameCollection != null)
            {
                foreach (ConfigurationProperty property in this._properties)
                {
                    if (((property.Name != "name") && (property.Name != "type")) && (this._PropertyNameCollection.Get(property.Name) == null))
                    {
                        if (list == null)
                        {
                            list = new ArrayList();
                        }
                        if ((base.Values.GetConfigValue(property.Name).ValueFlags & ConfigurationValueFlags.Locked) == ConfigurationValueFlags.Default)
                        {
                            list.Add(property.Name);
                            flag = true;
                        }
                    }
                }
                if (list != null)
                {
                    foreach (string str in list)
                    {
                        this._properties.Remove(str);
                    }
                }
                foreach (string str2 in this._PropertyNameCollection)
                {
                    string str3 = this._PropertyNameCollection[str2];
                    string str4 = this.GetProperty(str2);
                    if ((str4 == null) || (str3 != str4))
                    {
                        this.SetProperty(str2, str3);
                        flag = true;
                    }
                }
            }
            this._PropertyNameCollection = null;
            return flag;
        }

        [ConfigurationProperty("name", IsRequired=true, IsKey=true)]
        public string Name
        {
            get
            {
                return (string) base[this._propName];
            }
            set
            {
                base[this._propName] = value;
            }
        }

        public NameValueCollection Parameters
        {
            get
            {
                if (this._PropertyNameCollection == null)
                {
                    lock (this)
                    {
                        if (this._PropertyNameCollection == null)
                        {
                            this._PropertyNameCollection = new NameValueCollection(StringComparer.Ordinal);
                            foreach (object obj2 in this._properties)
                            {
                                ConfigurationProperty property = (ConfigurationProperty) obj2;
                                if ((property.Name != "name") && (property.Name != "type"))
                                {
                                    this._PropertyNameCollection.Add(property.Name, (string) base[property]);
                                }
                            }
                        }
                    }
                }
                return this._PropertyNameCollection;
            }
        }

        protected internal override ConfigurationPropertyCollection Properties
        {
            get
            {
                this.UpdatePropertyCollection();
                return this._properties;
            }
        }

        [ConfigurationProperty("type", IsRequired=true)]
        public string Type
        {
            get
            {
                return (string) base[this._propType];
            }
            set
            {
                base[this._propType] = value;
            }
        }
    }
}

