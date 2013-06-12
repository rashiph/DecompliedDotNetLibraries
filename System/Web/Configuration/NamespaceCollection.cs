namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Reflection;
    using System.Web.UI;

    [ConfigurationCollection(typeof(NamespaceInfo))]
    public sealed class NamespaceCollection : ConfigurationElementCollection
    {
        private Hashtable _namespaceEntries;
        private static readonly ConfigurationProperty _propAutoImportVBNamespace = new ConfigurationProperty("autoImportVBNamespace", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        static NamespaceCollection()
        {
            _properties.Add(_propAutoImportVBNamespace);
        }

        public void Add(NamespaceInfo namespaceInformation)
        {
            this.BaseAdd(namespaceInformation);
            this._namespaceEntries = null;
        }

        public void Clear()
        {
            base.BaseClear();
            this._namespaceEntries = null;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NamespaceInfo();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NamespaceInfo) element).Namespace;
        }

        public void Remove(string s)
        {
            base.BaseRemove(s);
            this._namespaceEntries = null;
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
            this._namespaceEntries = null;
        }

        [ConfigurationProperty("autoImportVBNamespace", DefaultValue=true)]
        public bool AutoImportVBNamespace
        {
            get
            {
                return (bool) base[_propAutoImportVBNamespace];
            }
            set
            {
                base[_propAutoImportVBNamespace] = value;
            }
        }

        public NamespaceInfo this[int index]
        {
            get
            {
                return (NamespaceInfo) base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
                this._namespaceEntries = null;
            }
        }

        internal Hashtable NamespaceEntries
        {
            get
            {
                if (this._namespaceEntries == null)
                {
                    lock (this)
                    {
                        if (this._namespaceEntries == null)
                        {
                            this._namespaceEntries = new Hashtable(StringComparer.OrdinalIgnoreCase);
                            foreach (NamespaceInfo info in this)
                            {
                                NamespaceEntry entry = new NamespaceEntry {
                                    Namespace = info.Namespace,
                                    Line = info.ElementInformation.Properties["namespace"].LineNumber,
                                    VirtualPath = info.ElementInformation.Properties["namespace"].Source
                                };
                                if (entry.Line == 0)
                                {
                                    entry.Line = 1;
                                }
                                this._namespaceEntries[info.Namespace] = entry;
                            }
                        }
                    }
                }
                return this._namespaceEntries;
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

