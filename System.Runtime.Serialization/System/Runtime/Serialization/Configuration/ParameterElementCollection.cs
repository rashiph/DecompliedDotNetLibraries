namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Runtime.Serialization;

    [ConfigurationCollection(typeof(ParameterElement), AddItemName="parameter", CollectionType=ConfigurationElementCollectionType.BasicMap)]
    public sealed class ParameterElementCollection : ConfigurationElementCollection
    {
        public ParameterElementCollection()
        {
            base.AddElementName = "parameter";
        }

        public void Add(ParameterElement element)
        {
            if (!this.IsReadOnly() && (element == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            this.BaseAdd(element);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public bool Contains(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
            }
            return (base.BaseGet(typeName) != null);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ParameterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return ((ParameterElement) element).identity;
        }

        public int IndexOf(ParameterElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return base.BaseIndexOf(element);
        }

        public void Remove(ParameterElement element)
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

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "parameter";
            }
        }

        public ParameterElement this[int index]
        {
            get
            {
                return (ParameterElement) base.BaseGet(index);
            }
            set
            {
                if (!this.IsReadOnly())
                {
                    if (value == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                    }
                    if (base.BaseGet(index) != null)
                    {
                        base.BaseRemoveAt(index);
                    }
                }
                this.BaseAdd(index, value);
            }
        }
    }
}

