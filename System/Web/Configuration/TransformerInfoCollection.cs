namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Reflection;
    using System.Web;
    using System.Web.UI.WebControls.WebParts;

    [ConfigurationCollection(typeof(TransformerInfo))]
    public sealed class TransformerInfoCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private Hashtable _transformerEntries;

        public void Add(TransformerInfo transformerInfo)
        {
            this.BaseAdd(transformerInfo);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TransformerInfo();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TransformerInfo) element).Name;
        }

        internal Hashtable GetTransformerEntries()
        {
            if (this._transformerEntries == null)
            {
                lock (this)
                {
                    if (this._transformerEntries == null)
                    {
                        this._transformerEntries = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        foreach (TransformerInfo info in this)
                        {
                            Type consumerType;
                            Type providerType;
                            Type transformerType = ConfigUtil.GetType(info.Type, "type", info);
                            if (!transformerType.IsSubclassOf(typeof(WebPartTransformer)))
                            {
                                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_doesnt_inherit_from_type", new object[] { info.Type, typeof(WebPartTransformer).FullName }), info.ElementInformation.Properties["type"].Source, info.ElementInformation.Properties["type"].LineNumber);
                            }
                            try
                            {
                                consumerType = WebPartTransformerAttribute.GetConsumerType(transformerType);
                                providerType = WebPartTransformerAttribute.GetProviderType(transformerType);
                            }
                            catch (Exception exception)
                            {
                                throw new ConfigurationErrorsException(System.Web.SR.GetString("Transformer_attribute_error", new object[] { exception.Message }), exception, info.ElementInformation.Properties["type"].Source, info.ElementInformation.Properties["type"].LineNumber);
                            }
                            if (this._transformerEntries.Count != 0)
                            {
                                foreach (DictionaryEntry entry in this._transformerEntries)
                                {
                                    Type type4 = (Type) entry.Value;
                                    Type type5 = WebPartTransformerAttribute.GetConsumerType(type4);
                                    Type type6 = WebPartTransformerAttribute.GetProviderType(type4);
                                    if ((consumerType == type5) && (providerType == type6))
                                    {
                                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Transformer_types_already_added", new object[] { (string) entry.Key, info.Name }), info.ElementInformation.Properties["type"].Source, info.ElementInformation.Properties["type"].LineNumber);
                                    }
                                }
                            }
                            this._transformerEntries[info.Name] = transformerType;
                        }
                    }
                }
            }
            return this._transformerEntries;
        }

        public void Remove(string s)
        {
            base.BaseRemove(s);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public TransformerInfo this[int index]
        {
            get
            {
                return (TransformerInfo) base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}

