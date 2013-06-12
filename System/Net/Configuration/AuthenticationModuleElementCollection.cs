namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(AuthenticationModuleElement))]
    public sealed class AuthenticationModuleElementCollection : ConfigurationElementCollection
    {
        public void Add(AuthenticationModuleElement element)
        {
            this.BaseAdd(element);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AuthenticationModuleElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return ((AuthenticationModuleElement) element).Key;
        }

        public int IndexOf(AuthenticationModuleElement element)
        {
            return base.BaseIndexOf(element);
        }

        public void Remove(AuthenticationModuleElement element)
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

        public AuthenticationModuleElement this[int index]
        {
            get
            {
                return (AuthenticationModuleElement) base.BaseGet(index);
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

        public AuthenticationModuleElement this[string name]
        {
            get
            {
                return (AuthenticationModuleElement) base.BaseGet(name);
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

