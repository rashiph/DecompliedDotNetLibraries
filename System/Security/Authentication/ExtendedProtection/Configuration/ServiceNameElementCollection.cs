namespace System.Security.Authentication.ExtendedProtection.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(ServiceNameElement))]
    public sealed class ServiceNameElementCollection : ConfigurationElementCollection
    {
        public void Add(ServiceNameElement element)
        {
            this.BaseAdd(element);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceNameElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return ((ServiceNameElement) element).Key;
        }

        public int IndexOf(ServiceNameElement element)
        {
            return base.BaseIndexOf(element);
        }

        public void Remove(ServiceNameElement element)
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

        public ServiceNameElement this[int index]
        {
            get
            {
                return (ServiceNameElement) base.BaseGet(index);
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

        public ServiceNameElement this[string name]
        {
            get
            {
                return (ServiceNameElement) base.BaseGet(name);
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
    }
}

