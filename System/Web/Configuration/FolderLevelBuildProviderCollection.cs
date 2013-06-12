namespace System.Web.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.Web.Compilation;

    [ConfigurationCollection(typeof(FolderLevelBuildProvider))]
    public sealed class FolderLevelBuildProviderCollection : ConfigurationElementCollection
    {
        private Dictionary<FolderLevelBuildProviderAppliesTo, List<Type>> _buildProviderMappings;
        private HashSet<Type> _buildProviderTypes;
        private bool _folderLevelBuildProviderTypesSet;
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public FolderLevelBuildProviderCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(FolderLevelBuildProvider buildProvider)
        {
            this.BaseAdd(buildProvider);
        }

        private void AddMapping(FolderLevelBuildProviderAppliesTo appliesTo, Type buildProviderType)
        {
            if (this._buildProviderMappings == null)
            {
                this._buildProviderMappings = new Dictionary<FolderLevelBuildProviderAppliesTo, List<Type>>();
            }
            if (this._buildProviderTypes == null)
            {
                this._buildProviderTypes = new HashSet<Type>();
            }
            List<Type> list = null;
            if (!this._buildProviderMappings.TryGetValue(appliesTo, out list))
            {
                list = new List<Type>();
                this._buildProviderMappings.Add(appliesTo, list);
            }
            list.Add(buildProviderType);
            this._buildProviderTypes.Add(buildProviderType);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FolderLevelBuildProvider();
        }

        private void EnsureFolderLevelBuildProvidersInitialized()
        {
            if (!this._folderLevelBuildProviderTypesSet)
            {
                lock (this)
                {
                    if (!this._folderLevelBuildProviderTypesSet)
                    {
                        foreach (FolderLevelBuildProvider provider in this)
                        {
                            this.AddMapping(provider.AppliesToInternal, provider.TypeInternal);
                        }
                        this._folderLevelBuildProviderTypesSet = true;
                    }
                }
            }
        }

        internal List<Type> GetBuildProviderTypes(FolderLevelBuildProviderAppliesTo appliesTo)
        {
            this.EnsureFolderLevelBuildProvidersInitialized();
            List<Type> list = new List<Type>();
            if (this._buildProviderMappings != null)
            {
                foreach (KeyValuePair<FolderLevelBuildProviderAppliesTo, List<Type>> pair in this._buildProviderMappings)
                {
                    if ((((FolderLevelBuildProviderAppliesTo) pair.Key) & appliesTo) != FolderLevelBuildProviderAppliesTo.None)
                    {
                        list.AddRange(pair.Value);
                    }
                }
            }
            return list;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FolderLevelBuildProvider) element).Name;
        }

        internal bool IsFolderLevelBuildProvider(Type t)
        {
            this.EnsureFolderLevelBuildProvidersInitialized();
            return ((this._buildProviderTypes != null) && this._buildProviderTypes.Contains(t));
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public System.Web.Configuration.BuildProvider this[string name]
        {
            get
            {
                return (System.Web.Configuration.BuildProvider) base.BaseGet(name);
            }
        }

        public FolderLevelBuildProvider this[int index]
        {
            get
            {
                return (FolderLevelBuildProvider) base.BaseGet(index);
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

