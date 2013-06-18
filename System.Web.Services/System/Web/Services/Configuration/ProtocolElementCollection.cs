namespace System.Web.Services.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Services;

    [ConfigurationCollection(typeof(ProtocolElement))]
    public sealed class ProtocolElementCollection : ConfigurationElementCollection
    {
        public void Add(ProtocolElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            this.BaseAdd(element);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public bool ContainsKey(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return (base.BaseGet(key) != null);
        }

        public void CopyTo(ProtocolElement[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            ((ICollection) this).CopyTo(array, index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProtocolElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            ProtocolElement element2 = (ProtocolElement) element;
            return element2.Name.ToString();
        }

        public int IndexOf(ProtocolElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return base.BaseIndexOf(element);
        }

        public void Remove(ProtocolElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            base.BaseRemove(this.GetElementKey(element));
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public void RemoveAt(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            base.BaseRemove(key);
        }

        internal void SetDefaults()
        {
            ProtocolElement element = new ProtocolElement(WebServiceProtocols.HttpSoap12);
            ProtocolElement element2 = new ProtocolElement(WebServiceProtocols.HttpSoap);
            ProtocolElement element3 = new ProtocolElement(WebServiceProtocols.HttpPostLocalhost);
            ProtocolElement element4 = new ProtocolElement(WebServiceProtocols.Documentation);
            this.Add(element);
            this.Add(element2);
            this.Add(element3);
            this.Add(element4);
        }

        public ProtocolElement this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                ProtocolElement element = (ProtocolElement) base.BaseGet(key);
                if (element == null)
                {
                    throw new KeyNotFoundException(string.Format(CultureInfo.InvariantCulture, Res.GetString("ConfigKeyNotFoundInElementCollection"), new object[] { key.ToString() }));
                }
                return element;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if (!this.GetElementKey(value).Equals(key))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Res.GetString("ConfigKeysDoNotMatch"), new object[] { this.GetElementKey(value).ToString(), key.ToString() }));
                }
                if (base.BaseGet(key) != null)
                {
                    base.BaseRemove(key);
                }
                this.Add(value);
            }
        }

        public ProtocolElement this[int index]
        {
            get
            {
                return (ProtocolElement) base.BaseGet(index);
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
    }
}

