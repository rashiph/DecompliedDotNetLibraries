namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(CodeSubDirectory), CollectionType=ConfigurationElementCollectionType.BasicMap)]
    public sealed class CodeSubDirectoriesCollection : ConfigurationElementCollection
    {
        private bool _didRuntimeValidation;
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public CodeSubDirectoriesCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(CodeSubDirectory codeSubDirectory)
        {
            this.BaseAdd(codeSubDirectory);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CodeSubDirectory();
        }

        internal void EnsureRuntimeValidation()
        {
            if (!this._didRuntimeValidation)
            {
                foreach (CodeSubDirectory directory in this)
                {
                    directory.DoRuntimeValidation();
                }
                this._didRuntimeValidation = true;
            }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CodeSubDirectory) element).DirectoryName;
        }

        public void Remove(string directoryName)
        {
            base.BaseRemove(directoryName);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "add";
            }
        }

        public CodeSubDirectory this[int index]
        {
            get
            {
                return (CodeSubDirectory) base.BaseGet(index);
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

