namespace System.Web.Services.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Services;

    [ConfigurationCollection(typeof(SoapExtensionTypeElement))]
    public sealed class SoapExtensionTypeElementCollection : ConfigurationElementCollection
    {
        public void Add(SoapExtensionTypeElement element)
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

        public void CopyTo(SoapExtensionTypeElement[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            ((ICollection) this).CopyTo(array, index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SoapExtensionTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return element;
        }

        public int IndexOf(SoapExtensionTypeElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return base.BaseIndexOf(element);
        }

        public void Remove(SoapExtensionTypeElement element)
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

        public SoapExtensionTypeElement this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                SoapExtensionTypeElement element = (SoapExtensionTypeElement) base.BaseGet(key);
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

        public SoapExtensionTypeElement this[int index]
        {
            get
            {
                return (SoapExtensionTypeElement) base.BaseGet(index);
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

