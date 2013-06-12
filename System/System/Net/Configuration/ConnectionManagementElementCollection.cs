namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(ConnectionManagementElement))]
    public sealed class ConnectionManagementElementCollection : ConfigurationElementCollection
    {
        public void Add(ConnectionManagementElement element)
        {
            this.BaseAdd(element);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ConnectionManagementElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return ((ConnectionManagementElement) element).Key;
        }

        public int IndexOf(ConnectionManagementElement element)
        {
            return base.BaseIndexOf(element);
        }

        public void Remove(ConnectionManagementElement element)
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

        public ConnectionManagementElement this[int index]
        {
            get
            {
                return (ConnectionManagementElement) base.BaseGet(index);
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

        public ConnectionManagementElement this[string name]
        {
            get
            {
                return (ConnectionManagementElement) base.BaseGet(name);
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

