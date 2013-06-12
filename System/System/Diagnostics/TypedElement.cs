namespace System.Diagnostics
{
    using System;
    using System.Configuration;

    internal class TypedElement : ConfigurationElement
    {
        private Type _baseType;
        protected ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        protected static readonly ConfigurationProperty _propInitData = new ConfigurationProperty("initializeData", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        protected static readonly ConfigurationProperty _propTypeName = new ConfigurationProperty("type", typeof(string), string.Empty, ConfigurationPropertyOptions.IsTypeStringTransformationRequired | ConfigurationPropertyOptions.IsRequired);
        protected object _runtimeObject;

        public TypedElement(Type baseType)
        {
            this._properties.Add(_propTypeName);
            this._properties.Add(_propInitData);
            this._baseType = baseType;
        }

        protected object BaseGetRuntimeObject()
        {
            if (this._runtimeObject == null)
            {
                this._runtimeObject = TraceUtils.GetRuntimeObject(this.TypeName, this._baseType, this.InitData);
            }
            return this._runtimeObject;
        }

        [ConfigurationProperty("initializeData", DefaultValue="")]
        public string InitData
        {
            get
            {
                return (string) base[_propInitData];
            }
            set
            {
                base[_propInitData] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this._properties;
            }
        }

        [ConfigurationProperty("type", IsRequired=true, DefaultValue="")]
        public virtual string TypeName
        {
            get
            {
                return (string) base[_propTypeName];
            }
            set
            {
                base[_propTypeName] = value;
            }
        }
    }
}

