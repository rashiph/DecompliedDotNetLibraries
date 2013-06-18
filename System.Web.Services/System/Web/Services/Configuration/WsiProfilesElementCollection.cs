namespace System.Web.Services.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Services;

    [ConfigurationCollection(typeof(WsiProfilesElement))]
    public sealed class WsiProfilesElementCollection : ConfigurationElementCollection
    {
        public void Add(WsiProfilesElement element)
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

        public void CopyTo(WsiProfilesElement[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            ((ICollection) this).CopyTo(array, index);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WsiProfilesElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            WsiProfilesElement element2 = (WsiProfilesElement) element;
            return element2.Name.ToString();
        }

        public int IndexOf(WsiProfilesElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return base.BaseIndexOf(element);
        }

        public void Remove(WsiProfilesElement element)
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
            WsiProfilesElement element = new WsiProfilesElement(WsiProfiles.BasicProfile1_1);
            this.Add(element);
        }

        public WsiProfilesElement this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                WsiProfilesElement element = (WsiProfilesElement) base.BaseGet(key);
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

        public WsiProfilesElement this[int index]
        {
            get
            {
                return (WsiProfilesElement) base.BaseGet(index);
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

