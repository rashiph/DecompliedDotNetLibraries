namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(ExpressionBuilder))]
    public sealed class ExpressionBuilderCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public ExpressionBuilderCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(ExpressionBuilder buildProvider)
        {
            this.BaseAdd(buildProvider);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ExpressionBuilder();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExpressionBuilder) element).ExpressionPrefix;
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public ExpressionBuilder this[string name]
        {
            get
            {
                return (ExpressionBuilder) base.BaseGet(name);
            }
        }

        public ExpressionBuilder this[int index]
        {
            get
            {
                return (ExpressionBuilder) base.BaseGet(index);
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

