namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.ServiceModel;
    using System.Text;

    public abstract class ServiceModelConfigurationElementCollection<ConfigurationElementType> : ConfigurationElementCollection where ConfigurationElementType: ConfigurationElement, new()
    {
        private ConfigurationElementCollectionType collectionType;
        private string elementName;

        internal ServiceModelConfigurationElementCollection() : this(ConfigurationElementCollectionType.AddRemoveClearMap, null)
        {
        }

        internal ServiceModelConfigurationElementCollection(ConfigurationElementCollectionType collectionType, string elementName)
        {
            this.collectionType = collectionType;
            this.elementName = elementName;
            if (!string.IsNullOrEmpty(elementName))
            {
                base.AddElementName = elementName;
            }
        }

        internal ServiceModelConfigurationElementCollection(ConfigurationElementCollectionType collectionType, string elementName, IComparer comparer) : base(comparer)
        {
            this.collectionType = collectionType;
            this.elementName = elementName;
        }

        public void Add(ConfigurationElementType element)
        {
            if (!this.IsReadOnly() && (element == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            this.BaseAdd(element);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            if (!this.IsReadOnly() && !this.ThrowOnDuplicate)
            {
                object elementKey = this.GetElementKey(element);
                if (this.ContainsKey(elementKey))
                {
                    base.BaseRemove(elementKey);
                }
            }
            base.BaseAdd(element);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public virtual bool ContainsKey(object key)
        {
            if (key != null)
            {
                return (null != base.BaseGet(key));
            }
            List<string> list = new List<string>();
            foreach (PropertyInformation information in this.CreateNewElement().ElementInformation.Properties)
            {
                if (information.IsKey)
                {
                    list.Add(information.Name);
                }
            }
            if (list.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            if (1 == list.Count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigElementKeyNull", new object[] { list[0] })));
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < (list.Count - 1); i++)
            {
                builder = builder.Append(list[i] + ", ");
            }
            builder = builder.Append(list[list.Count - 1]);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigElementKeysNull", new object[] { list.ToString() })));
        }

        public void CopyTo(ConfigurationElementType[] array, int start)
        {
            if (array == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("array");
            }
            if ((start < 0) || (start >= array.Length))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("start", System.ServiceModel.SR.GetString("ConfigInvalidStartValue", new object[] { array.Length - 1, start }));
            }
            ((ICollection) this).CopyTo(array, start);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return Activator.CreateInstance<ConfigurationElementType>();
        }

        public int IndexOf(ConfigurationElementType element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return base.BaseIndexOf(element);
        }

        public void Remove(ConfigurationElementType element)
        {
            if (!this.IsReadOnly() && (element == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            base.BaseRemove(this.GetElementKey(element));
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public void RemoveAt(object key)
        {
            if (!this.IsReadOnly() && (key == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            base.BaseRemove(key);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return this.collectionType;
            }
        }

        protected override string ElementName
        {
            get
            {
                string elementName = this.elementName;
                if (string.IsNullOrEmpty(elementName))
                {
                    elementName = base.ElementName;
                }
                return elementName;
            }
        }

        public virtual ConfigurationElementType this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }
                ConfigurationElementType local = (ConfigurationElementType) base.BaseGet(key);
                if (local == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new KeyNotFoundException(System.ServiceModel.SR.GetString("ConfigKeyNotFoundInElementCollection", new object[] { key.ToString() })));
                }
                return local;
            }
            set
            {
                if (this.IsReadOnly())
                {
                    this.Add(value);
                }
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }
                if (!this.GetElementKey(value).ToString().Equals((string) key, StringComparison.Ordinal))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigKeysDoNotMatch", new object[] { this.GetElementKey(value).ToString(), key.ToString() }));
                }
                if (base.BaseGet(key) != null)
                {
                    base.BaseRemove(key);
                }
                this.Add(value);
            }
        }

        public ConfigurationElementType this[int index]
        {
            get
            {
                return (ConfigurationElementType) base.BaseGet(index);
            }
            set
            {
                if ((!this.IsReadOnly() && !this.ThrowOnDuplicate) && (base.BaseGet(index) != null))
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
    }
}

