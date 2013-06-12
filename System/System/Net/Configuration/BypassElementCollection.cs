namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(BypassElement))]
    public sealed class BypassElementCollection : ConfigurationElementCollection
    {
        public void Add(BypassElement element)
        {
            this.BaseAdd(element);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new BypassElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return ((BypassElement) element).Key;
        }

        public int IndexOf(BypassElement element)
        {
            return base.BaseIndexOf(element);
        }

        public void Remove(BypassElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            base.BaseRemove(element.Key);
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public BypassElement this[int index]
        {
            get
            {
                return (BypassElement) base.BaseGet(index);
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

        public BypassElement this[string name]
        {
            get
            {
                return (BypassElement) base.BaseGet(name);
            }
            set
            {
                if (base.BaseGet(name) != null)
                {
                    base.BaseRemove(name);
                }
                this.BaseAdd(value);
            }
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return false;
            }
        }
    }
}

