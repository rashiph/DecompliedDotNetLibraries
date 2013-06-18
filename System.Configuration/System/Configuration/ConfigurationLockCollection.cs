namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Text;

    public sealed class ConfigurationLockCollection : ICollection, IEnumerable
    {
        private bool _bExceptionList;
        private bool _bModified;
        private string _ignoreName;
        private ConfigurationLockCollectionType _lockType;
        private ConfigurationElement _thisElement;
        private ArrayList internalArraylist;
        private HybridDictionary internalDictionary;
        private const string LockAll = "*";
        private string SeedList;

        internal ConfigurationLockCollection(ConfigurationElement thisElement) : this(thisElement, ConfigurationLockCollectionType.LockedAttributes)
        {
        }

        internal ConfigurationLockCollection(ConfigurationElement thisElement, ConfigurationLockCollectionType lockType) : this(thisElement, lockType, string.Empty)
        {
        }

        internal ConfigurationLockCollection(ConfigurationElement thisElement, ConfigurationLockCollectionType lockType, string ignoreName) : this(thisElement, lockType, ignoreName, null)
        {
        }

        internal ConfigurationLockCollection(ConfigurationElement thisElement, ConfigurationLockCollectionType lockType, string ignoreName, ConfigurationLockCollection parentCollection)
        {
            this._ignoreName = string.Empty;
            this.SeedList = string.Empty;
            this._thisElement = thisElement;
            this._lockType = lockType;
            this.internalDictionary = new HybridDictionary();
            this.internalArraylist = new ArrayList();
            this._bModified = false;
            this._bExceptionList = (this._lockType == ConfigurationLockCollectionType.LockedExceptionList) || (this._lockType == ConfigurationLockCollectionType.LockedElementsExceptionList);
            this._ignoreName = ignoreName;
            if (parentCollection != null)
            {
                foreach (string str in parentCollection)
                {
                    this.Add(str, ConfigurationValueFlags.Inherited);
                    if (this._bExceptionList)
                    {
                        if (this.SeedList.Length != 0)
                        {
                            this.SeedList = this.SeedList + ",";
                        }
                        this.SeedList = this.SeedList + str;
                    }
                }
            }
        }

        public void Add(string name)
        {
            if (((this._thisElement.ItemLocked & ConfigurationValueFlags.Locked) != ConfigurationValueFlags.Default) && ((this._thisElement.ItemLocked & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_attribute_locked", new object[] { name }));
            }
            ConfigurationValueFlags modified = ConfigurationValueFlags.Modified;
            string attribToLockTrim = name.Trim();
            ConfigurationProperty property = this._thisElement.Properties[attribToLockTrim];
            if ((property == null) && (attribToLockTrim != "*"))
            {
                ConfigurationElementCollection elements = this._thisElement as ConfigurationElementCollection;
                if ((elements == null) && (this._thisElement.Properties.DefaultCollectionProperty != null))
                {
                    elements = this._thisElement[this._thisElement.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                }
                if (((elements == null) || (this._lockType == ConfigurationLockCollectionType.LockedAttributes)) || (this._lockType == ConfigurationLockCollectionType.LockedExceptionList))
                {
                    this._thisElement.ReportInvalidLock(attribToLockTrim, this._lockType, null, null);
                }
                else if (!elements.IsLockableElement(attribToLockTrim))
                {
                    this._thisElement.ReportInvalidLock(attribToLockTrim, this._lockType, null, elements.LockableElements);
                }
            }
            else
            {
                if ((property != null) && property.IsRequired)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_required_attribute_lock_attempt", new object[] { property.Name }));
                }
                if (attribToLockTrim != "*")
                {
                    if ((this._lockType == ConfigurationLockCollectionType.LockedElements) || (this._lockType == ConfigurationLockCollectionType.LockedElementsExceptionList))
                    {
                        if (!typeof(ConfigurationElement).IsAssignableFrom(property.Type))
                        {
                            this._thisElement.ReportInvalidLock(attribToLockTrim, this._lockType, null, null);
                        }
                    }
                    else if (typeof(ConfigurationElement).IsAssignableFrom(property.Type))
                    {
                        this._thisElement.ReportInvalidLock(attribToLockTrim, this._lockType, null, null);
                    }
                }
            }
            if (this.internalDictionary.Contains(name))
            {
                modified = ConfigurationValueFlags.Modified | ((ConfigurationValueFlags) this.internalDictionary[name]);
                this.internalDictionary.Remove(name);
                this.internalArraylist.Remove(name);
            }
            this.internalDictionary.Add(name, modified);
            this.internalArraylist.Add(name);
            this._bModified = true;
        }

        internal void Add(string name, ConfigurationValueFlags flags)
        {
            if ((flags != ConfigurationValueFlags.Inherited) && this.internalDictionary.Contains(name))
            {
                flags = ConfigurationValueFlags.Modified | ((ConfigurationValueFlags) this.internalDictionary[name]);
                this.internalDictionary.Remove(name);
                this.internalArraylist.Remove(name);
            }
            this.internalDictionary.Add(name, flags);
            this.internalArraylist.Add(name);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Clear()
        {
            this.ClearInternal(true);
        }

        internal void ClearInternal(bool useSeedIfAvailble)
        {
            ArrayList list = new ArrayList();
            foreach (DictionaryEntry entry in this.internalDictionary)
            {
                if (((((ConfigurationValueFlags) entry.Value) & ConfigurationValueFlags.Inherited) == ConfigurationValueFlags.Default) || this._bExceptionList)
                {
                    list.Add(entry.Key);
                }
            }
            foreach (object obj2 in list)
            {
                this.internalDictionary.Remove(obj2);
                this.internalArraylist.Remove(obj2);
            }
            if (useSeedIfAvailble && !string.IsNullOrEmpty(this.SeedList))
            {
                foreach (string str in this.SeedList.Split(new char[] { ',' }))
                {
                    this.Add(str, ConfigurationValueFlags.Inherited);
                }
            }
            this._bModified = true;
        }

        internal void ClearSeedList()
        {
            this.SeedList = string.Empty;
        }

        public bool Contains(string name)
        {
            return ((this._bExceptionList && name.Equals(this._ignoreName)) || this.internalDictionary.Contains(name));
        }

        public void CopyTo(string[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        internal bool DefinedInParent(string name)
        {
            if (name == null)
            {
                return false;
            }
            if (this._bExceptionList)
            {
                string str = "," + this.SeedList + ",";
                if (name.Equals(this._ignoreName) || (str.IndexOf("," + name + ",", StringComparison.Ordinal) >= 0))
                {
                    return true;
                }
            }
            return (this.internalDictionary.Contains(name) && ((((ConfigurationValueFlags) this.internalDictionary[name]) & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default));
        }

        public IEnumerator GetEnumerator()
        {
            return this.internalArraylist.GetEnumerator();
        }

        public bool IsReadOnly(string name)
        {
            if (!this.internalDictionary.Contains(name))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_entry_not_found", new object[] { name }));
            }
            return ((((ConfigurationValueFlags) this.internalDictionary[name]) & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default);
        }

        internal bool IsValueModified(string name)
        {
            return (this.internalDictionary.Contains(name) && ((((ConfigurationValueFlags) this.internalDictionary[name]) & ConfigurationValueFlags.Modified) != ConfigurationValueFlags.Default));
        }

        public void Remove(string name)
        {
            if (!this.internalDictionary.Contains(name))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_entry_not_found", new object[] { name }));
            }
            if (!this._bExceptionList && ((((ConfigurationValueFlags) this.internalDictionary[name]) & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default))
            {
                if ((((ConfigurationValueFlags) this.internalDictionary[name]) & ConfigurationValueFlags.Modified) == ConfigurationValueFlags.Default)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_attribute_locked", new object[] { name }));
                }
                ConfigurationValueFlags flags = (ConfigurationValueFlags) this.internalDictionary[name];
                flags &= ~ConfigurationValueFlags.Modified;
                this.internalDictionary[name] = flags;
                this._bModified = true;
            }
            else
            {
                this.internalDictionary.Remove(name);
                this.internalArraylist.Remove(name);
                this._bModified = true;
            }
        }

        internal void RemoveInheritedLocks()
        {
            StringCollection strings = new StringCollection();
            foreach (string str in this)
            {
                if (this.DefinedInParent(str))
                {
                    strings.Add(str);
                }
            }
            foreach (string str2 in strings)
            {
                this.internalDictionary.Remove(str2);
                this.internalArraylist.Remove(str2);
            }
        }

        internal void ResetModified()
        {
            this._bModified = false;
        }

        public void SetFromList(string attributeList)
        {
            string[] strArray = attributeList.Split(new char[] { ',', ';', ':' });
            this.Clear();
            foreach (string str in strArray)
            {
                string name = str.Trim();
                if (!this.Contains(name))
                {
                    this.Add(name);
                }
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.internalArraylist.CopyTo(array, index);
        }

        public string AttributeList
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (DictionaryEntry entry in this.internalDictionary)
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(',');
                    }
                    builder.Append(entry.Key);
                }
                return builder.ToString();
            }
        }

        public int Count
        {
            get
            {
                return this.internalDictionary.Count;
            }
        }

        internal bool ExceptionList
        {
            get
            {
                return this._bExceptionList;
            }
        }

        public bool HasParentElements
        {
            get
            {
                if ((this.ExceptionList && (this.internalDictionary.Count == 0)) && !string.IsNullOrEmpty(this.SeedList))
                {
                    return true;
                }
                foreach (DictionaryEntry entry in this.internalDictionary)
                {
                    if ((((ConfigurationValueFlags) entry.Value) & ConfigurationValueFlags.Inherited) != ConfigurationValueFlags.Default)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsModified
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._bModified;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        internal ConfigurationLockCollectionType LockType
        {
            get
            {
                return this._lockType;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

