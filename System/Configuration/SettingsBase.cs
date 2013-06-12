namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    public abstract class SettingsBase
    {
        private SettingsContext _Context;
        private bool _IsSynchronized;
        private SettingsPropertyCollection _Properties;
        private SettingsPropertyValueCollection _PropertyValues = new SettingsPropertyValueCollection();
        private SettingsProviderCollection _Providers;

        protected SettingsBase()
        {
        }

        private void GetPropertiesFromProvider(SettingsProvider provider)
        {
            SettingsPropertyCollection collection = new SettingsPropertyCollection();
            foreach (SettingsProperty property in this.Properties)
            {
                if (property.Provider == provider)
                {
                    collection.Add(property);
                }
            }
            if (collection.Count > 0)
            {
                foreach (SettingsPropertyValue value2 in provider.GetPropertyValues(this.Context, collection))
                {
                    if (this._PropertyValues[value2.Name] == null)
                    {
                        this._PropertyValues.Add(value2);
                    }
                }
            }
        }

        private object GetPropertyValueByName(string propertyName)
        {
            if (((this.Properties == null) || (this._PropertyValues == null)) || (this.Properties.Count == 0))
            {
                throw new SettingsPropertyNotFoundException(System.SR.GetString("SettingsPropertyNotFound", new object[] { propertyName }));
            }
            SettingsProperty property = this.Properties[propertyName];
            if (property == null)
            {
                throw new SettingsPropertyNotFoundException(System.SR.GetString("SettingsPropertyNotFound", new object[] { propertyName }));
            }
            SettingsPropertyValue value2 = this._PropertyValues[propertyName];
            if (value2 == null)
            {
                this.GetPropertiesFromProvider(property.Provider);
                value2 = this._PropertyValues[propertyName];
                if (value2 == null)
                {
                    throw new SettingsPropertyNotFoundException(System.SR.GetString("SettingsPropertyNotFound", new object[] { propertyName }));
                }
            }
            return value2.PropertyValue;
        }

        public void Initialize(SettingsContext context, SettingsPropertyCollection properties, SettingsProviderCollection providers)
        {
            this._Context = context;
            this._Properties = properties;
            this._Providers = providers;
        }

        public virtual void Save()
        {
            if (this.IsSynchronized)
            {
                lock (this)
                {
                    this.SaveCore();
                    return;
                }
            }
            this.SaveCore();
        }

        private void SaveCore()
        {
            if (((this.Properties != null) && (this._PropertyValues != null)) && (this.Properties.Count != 0))
            {
                foreach (SettingsProvider provider in this.Providers)
                {
                    SettingsPropertyValueCollection collection = new SettingsPropertyValueCollection();
                    foreach (SettingsPropertyValue value2 in this.PropertyValues)
                    {
                        if (value2.Property.Provider == provider)
                        {
                            collection.Add(value2);
                        }
                    }
                    if (collection.Count > 0)
                    {
                        provider.SetPropertyValues(this.Context, collection);
                    }
                }
                foreach (SettingsPropertyValue value3 in this.PropertyValues)
                {
                    value3.IsDirty = false;
                }
            }
        }

        private void SetPropertyValueByName(string propertyName, object propertyValue)
        {
            if (((this.Properties == null) || (this._PropertyValues == null)) || (this.Properties.Count == 0))
            {
                throw new SettingsPropertyNotFoundException(System.SR.GetString("SettingsPropertyNotFound", new object[] { propertyName }));
            }
            SettingsProperty property = this.Properties[propertyName];
            if (property == null)
            {
                throw new SettingsPropertyNotFoundException(System.SR.GetString("SettingsPropertyNotFound", new object[] { propertyName }));
            }
            if (property.IsReadOnly)
            {
                throw new SettingsPropertyIsReadOnlyException(System.SR.GetString("SettingsPropertyReadOnly", new object[] { propertyName }));
            }
            if ((propertyValue != null) && !property.PropertyType.IsInstanceOfType(propertyValue))
            {
                throw new SettingsPropertyWrongTypeException(System.SR.GetString("SettingsPropertyWrongType", new object[] { propertyName }));
            }
            SettingsPropertyValue value2 = this._PropertyValues[propertyName];
            if (value2 == null)
            {
                this.GetPropertiesFromProvider(property.Provider);
                value2 = this._PropertyValues[propertyName];
                if (value2 == null)
                {
                    throw new SettingsPropertyNotFoundException(System.SR.GetString("SettingsPropertyNotFound", new object[] { propertyName }));
                }
            }
            value2.PropertyValue = propertyValue;
        }

        public static SettingsBase Synchronized(SettingsBase settingsBase)
        {
            settingsBase._IsSynchronized = true;
            return settingsBase;
        }

        public virtual SettingsContext Context
        {
            get
            {
                return this._Context;
            }
        }

        [Browsable(false)]
        public bool IsSynchronized
        {
            get
            {
                return this._IsSynchronized;
            }
        }

        public virtual object this[string propertyName]
        {
            get
            {
                if (this.IsSynchronized)
                {
                    lock (this)
                    {
                        return this.GetPropertyValueByName(propertyName);
                    }
                }
                return this.GetPropertyValueByName(propertyName);
            }
            set
            {
                if (this.IsSynchronized)
                {
                    lock (this)
                    {
                        this.SetPropertyValueByName(propertyName, value);
                        return;
                    }
                }
                this.SetPropertyValueByName(propertyName, value);
            }
        }

        public virtual SettingsPropertyCollection Properties
        {
            get
            {
                return this._Properties;
            }
        }

        public virtual SettingsPropertyValueCollection PropertyValues
        {
            get
            {
                return this._PropertyValues;
            }
        }

        public virtual SettingsProviderCollection Providers
        {
            get
            {
                return this._Providers;
            }
        }
    }
}

