namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Web.Util;

    [ConfigurationCollection(typeof(ClientTarget))]
    public sealed class ClientTargetCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public ClientTargetCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(ClientTarget clientTarget)
        {
            this.BaseAdd(clientTarget);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ClientTarget();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ClientTarget) element).Alias;
        }

        public string GetKey(int index)
        {
            return (string) base.BaseGetKey(index);
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void Remove(ClientTarget clientTarget)
        {
            base.BaseRemove(this.GetElementKey(clientTarget));
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public string[] AllKeys
        {
            get
            {
                return System.Web.Util.StringUtil.ObjectArrayToStringArray(base.BaseGetAllKeys());
            }
        }

        public ClientTarget this[string name]
        {
            get
            {
                return (ClientTarget) base.BaseGet(name);
            }
        }

        public ClientTarget this[int index]
        {
            get
            {
                return (ClientTarget) base.BaseGet(index);
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

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}

