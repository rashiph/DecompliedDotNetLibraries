namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Runtime.Serialization;

    [ConfigurationCollection(typeof(DeclaredTypeElement))]
    public sealed class DeclaredTypeElementCollection : ConfigurationElementCollection
    {
        public void Add(DeclaredTypeElement element)
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
            return new DeclaredTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return ((DeclaredTypeElement) element).Type;
        }

        public int IndexOf(DeclaredTypeElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return base.BaseIndexOf(element);
        }

        public void Remove(DeclaredTypeElement element)
        {
            if (!this.IsReadOnly() && (element == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            base.BaseRemove(this.GetElementKey(element));
        }

        public void Remove(string typeName)
        {
            if (!this.IsReadOnly() && string.IsNullOrEmpty(typeName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
            }
            base.BaseRemove(typeName);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public DeclaredTypeElement this[int index]
        {
            get
            {
                return (DeclaredTypeElement) base.BaseGet(index);
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

        public DeclaredTypeElement this[string typeName]
        {
            get
            {
                if (string.IsNullOrEmpty(typeName))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
                }
                return (DeclaredTypeElement) base.BaseGet(typeName);
            }
            set
            {
                if (!this.IsReadOnly())
                {
                    if (string.IsNullOrEmpty(typeName))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
                    }
                    if (value == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                    }
                    if (base.BaseGet(typeName) == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new IndexOutOfRangeException(System.Runtime.Serialization.SR.GetString("ConfigIndexOutOfRange", new object[] { typeName })));
                    }
                    base.BaseRemove(typeName);
                }
                this.Add(value);
            }
        }
    }
}

