namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Runtime.Serialization;

    [ConfigurationCollection(typeof(TypeElement), CollectionType=ConfigurationElementCollectionType.BasicMap)]
    public sealed class TypeElementCollection : ConfigurationElementCollection
    {
        private const string KnownTypeConfig = "knownType";

        public void Add(TypeElement element)
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

        protected override ConfigurationElement CreateNewElement()
        {
            return new TypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return ((TypeElement) element).Key;
        }

        public int IndexOf(TypeElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return base.BaseIndexOf(element);
        }

        public void Remove(TypeElement element)
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
                return "knownType";
            }
        }

        public TypeElement this[int index]
        {
            get
            {
                return (TypeElement) base.BaseGet(index);
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

