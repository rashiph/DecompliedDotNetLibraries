namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime;
    using System.Xml;

    [DebuggerDisplay("Count = {Count}")]
    public abstract class ConfigurationElementCollection : ConfigurationElement, ICollection, IEnumerable
    {
        private string _addElement;
        private string _clearElement;
        private IComparer _comparer;
        private int _inheritedCount;
        private ArrayList _items;
        private int _removedItemCount;
        private string _removeElement;
        private bool bCollectionCleared;
        private bool bEmitClearTag;
        private bool bModified;
        private bool bReadOnly;
        internal const string DefaultAddItemName = "add";
        internal const string DefaultClearItemsName = "clear";
        internal const string DefaultRemoveItemName = "remove";
        internal bool internalAddToEnd;
        internal string internalElementTagName;

        protected ConfigurationElementCollection()
        {
            this._items = new ArrayList();
            this._addElement = "add";
            this._removeElement = "remove";
            this._clearElement = "clear";
            this.internalElementTagName = string.Empty;
        }

        protected ConfigurationElementCollection(IComparer comparer)
        {
            this._items = new ArrayList();
            this._addElement = "add";
            this._removeElement = "remove";
            this._clearElement = "clear";
            this.internalElementTagName = string.Empty;
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this._comparer = comparer;
        }

        internal override void AssociateContext(BaseConfigurationRecord configRecord)
        {
            base.AssociateContext(configRecord);
            foreach (Entry entry in this._items)
            {
                if (entry._value != null)
                {
                    entry._value.AssociateContext(configRecord);
                }
            }
        }

        protected virtual void BaseAdd(ConfigurationElement element)
        {
            this.BaseAdd(element, this.ThrowOnDuplicate);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected internal void BaseAdd(ConfigurationElement element, bool throwIfExists)
        {
            this.BaseAdd(element, throwIfExists, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected virtual void BaseAdd(int index, ConfigurationElement element)
        {
            this.BaseAdd(index, element, false);
        }

        private void BaseAdd(ConfigurationElement element, bool throwIfExists, bool ignoreLocks)
        {
            bool flagAsReplaced = false;
            bool internalAddToEnd = this.internalAddToEnd;
            if (this.IsReadOnly())
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_read_only"));
            }
            if (base.LockItem && !ignoreLocks)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_element_locked", new object[] { this._addElement }));
            }
            object elementKeyInternal = this.GetElementKeyInternal(element);
            int index = -1;
            for (int i = 0; i < this._items.Count; i++)
            {
                Entry entry = (Entry) this._items[i];
                if (this.CompareKeys(elementKeyInternal, entry.GetKey(this)))
                {
                    if (((entry._value != null) && entry._value.LockItem) && !ignoreLocks)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_item_locked"));
                    }
                    if ((entry._entryType != EntryType.Removed) && throwIfExists)
                    {
                        if (!element.Equals(entry._value))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_entry_already_exists", new object[] { elementKeyInternal }), element.PropertyFileName(""), element.PropertyLineNumber(""));
                        }
                        entry._value = element;
                        return;
                    }
                    if (entry._entryType != EntryType.Added)
                    {
                        if (((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap) || (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate)) && ((entry._entryType == EntryType.Removed) && (this._removedItemCount > 0)))
                        {
                            this._removedItemCount--;
                        }
                        entry._entryType = EntryType.Replaced;
                        flagAsReplaced = true;
                    }
                    if (!internalAddToEnd && (this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
                    {
                        if (!ignoreLocks)
                        {
                            element.HandleLockedAttributes(entry._value);
                            element.MergeLocks(entry._value);
                        }
                        entry._value = element;
                        this.bModified = true;
                        return;
                    }
                    index = i;
                    if (entry._entryType == EntryType.Added)
                    {
                        internalAddToEnd = true;
                    }
                    break;
                }
            }
            if (index >= 0)
            {
                this._items.RemoveAt(index);
                if ((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) && (index > ((this.Count + this._removedItemCount) - this._inheritedCount)))
                {
                    this._inheritedCount--;
                }
            }
            this.BaseAddInternal(internalAddToEnd ? -1 : index, element, flagAsReplaced, ignoreLocks);
            this.bModified = true;
        }

        private void BaseAdd(int index, ConfigurationElement element, bool ignoreLocks)
        {
            if (this.IsReadOnly())
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_read_only"));
            }
            if (index < -1)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("IndexOutOfRange", new object[] { index }));
            }
            if ((index != -1) && ((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap) || (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate)))
            {
                int num = 0;
                if (index > 0)
                {
                    foreach (Entry entry in this._items)
                    {
                        if (entry._entryType != EntryType.Removed)
                        {
                            index--;
                        }
                        if (index == 0)
                        {
                            break;
                        }
                        num++;
                    }
                    index = ++num;
                }
                object elementKeyInternal = this.GetElementKeyInternal(element);
                foreach (Entry entry2 in this._items)
                {
                    if (this.CompareKeys(elementKeyInternal, entry2.GetKey(this)) && (entry2._entryType != EntryType.Removed))
                    {
                        if (!element.Equals(entry2._value))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_entry_already_exists", new object[] { elementKeyInternal }), element.PropertyFileName(""), element.PropertyLineNumber(""));
                        }
                        return;
                    }
                }
            }
            this.BaseAddInternal(index, element, false, ignoreLocks);
        }

        private void BaseAddInternal(int index, ConfigurationElement element, bool flagAsReplaced, bool ignoreLocks)
        {
            element.AssociateContext(base._configRecord);
            if (element != null)
            {
                element.CallInit();
            }
            if (this.IsReadOnly())
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_read_only"));
            }
            if (!ignoreLocks)
            {
                if ((this.CollectionType == ConfigurationElementCollectionType.BasicMap) || (this.CollectionType == ConfigurationElementCollectionType.BasicMapAlternate))
                {
                    if (BaseConfigurationRecord.IsReservedAttributeName(this.ElementName))
                    {
                        throw new ArgumentException(System.Configuration.SR.GetString("Basicmap_item_name_reserved", new object[] { this.ElementName }));
                    }
                    base.CheckLockedElement(this.ElementName, null);
                }
                if ((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap) || (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
                {
                    base.CheckLockedElement(this._addElement, null);
                }
            }
            if ((this.CollectionType == ConfigurationElementCollectionType.BasicMapAlternate) || (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
            {
                if (index == -1)
                {
                    index = (this.Count + this._removedItemCount) - this._inheritedCount;
                }
                else if ((index > ((this.Count + this._removedItemCount) - this._inheritedCount)) && !flagAsReplaced)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_cannot_add_items_below_inherited_items"));
                }
            }
            if (((this.CollectionType == ConfigurationElementCollectionType.BasicMap) && (index >= 0)) && (index < this._inheritedCount))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_cannot_add_items_above_inherited_items"));
            }
            EntryType type = !flagAsReplaced ? EntryType.Added : EntryType.Replaced;
            object elementKeyInternal = this.GetElementKeyInternal(element);
            if (index >= 0)
            {
                if (index > this._items.Count)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("IndexOutOfRange", new object[] { index }));
                }
                this._items.Insert(index, new Entry(type, elementKeyInternal, element));
            }
            else
            {
                this._items.Add(new Entry(type, elementKeyInternal, element));
            }
            this.bModified = true;
        }

        protected internal void BaseClear()
        {
            if (this.IsReadOnly())
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_read_only"));
            }
            base.CheckLockedElement(this._clearElement, null);
            base.CheckLockedElement(this._removeElement, null);
            this.bModified = true;
            this.bCollectionCleared = true;
            if (((this.CollectionType == ConfigurationElementCollectionType.BasicMap) || (this.CollectionType == ConfigurationElementCollectionType.BasicMapAlternate)) && (this._inheritedCount > 0))
            {
                int index = 0;
                if (this.CollectionType == ConfigurationElementCollectionType.BasicMapAlternate)
                {
                    index = 0;
                }
                if (this.CollectionType == ConfigurationElementCollectionType.BasicMap)
                {
                    index = this._inheritedCount;
                }
                while ((this.Count - this._inheritedCount) > 0)
                {
                    this._items.RemoveAt(index);
                }
            }
            else
            {
                int num2 = 0;
                int num3 = 0;
                int count = this.Count;
                for (int i = 0; i < this._items.Count; i++)
                {
                    Entry entry = (Entry) this._items[i];
                    if ((entry._value != null) && entry._value.LockItem)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_item_locked_cannot_clear"));
                    }
                }
                for (int j = this._items.Count - 1; j >= 0; j--)
                {
                    Entry entry2 = (Entry) this._items[j];
                    if (((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap) && (j < this._inheritedCount)) || ((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) && (j >= (count - this._inheritedCount))))
                    {
                        num2++;
                    }
                    if (entry2._entryType == EntryType.Removed)
                    {
                        num3++;
                    }
                    this._items.RemoveAt(j);
                }
                this._inheritedCount -= num2;
                this._removedItemCount -= num3;
            }
        }

        protected internal ConfigurationElement BaseGet(int index)
        {
            if (index < 0)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("IndexOutOfRange", new object[] { index }));
            }
            int num = 0;
            Entry entry = null;
            foreach (Entry entry2 in this._items)
            {
                if ((num == index) && (entry2._entryType != EntryType.Removed))
                {
                    entry = entry2;
                    break;
                }
                if (entry2._entryType != EntryType.Removed)
                {
                    num++;
                }
            }
            if (entry == null)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("IndexOutOfRange", new object[] { index }));
            }
            return entry._value;
        }

        protected internal ConfigurationElement BaseGet(object key)
        {
            foreach (Entry entry in this._items)
            {
                if ((entry._entryType != EntryType.Removed) && this.CompareKeys(key, entry.GetKey(this)))
                {
                    return entry._value;
                }
            }
            return null;
        }

        protected internal object[] BaseGetAllKeys()
        {
            object[] objArray = new object[this.Count];
            int index = 0;
            foreach (Entry entry in this._items)
            {
                if (entry._entryType != EntryType.Removed)
                {
                    objArray[index] = entry.GetKey(this);
                    index++;
                }
            }
            return objArray;
        }

        protected internal object BaseGetKey(int index)
        {
            int num = 0;
            Entry entry = null;
            if (index < 0)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("IndexOutOfRange", new object[] { index }));
            }
            foreach (Entry entry2 in this._items)
            {
                if ((num == index) && (entry2._entryType != EntryType.Removed))
                {
                    entry = entry2;
                    break;
                }
                if (entry2._entryType != EntryType.Removed)
                {
                    num++;
                }
            }
            if (entry == null)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("IndexOutOfRange", new object[] { index }));
            }
            return entry.GetKey(this);
        }

        protected int BaseIndexOf(ConfigurationElement element)
        {
            int num = 0;
            object elementKeyInternal = this.GetElementKeyInternal(element);
            foreach (Entry entry in this._items)
            {
                if (entry._entryType != EntryType.Removed)
                {
                    if (this.CompareKeys(elementKeyInternal, entry.GetKey(this)))
                    {
                        return num;
                    }
                    num++;
                }
            }
            return -1;
        }

        protected internal bool BaseIsRemoved(object key)
        {
            foreach (Entry entry in this._items)
            {
                if (this.CompareKeys(key, entry.GetKey(this)))
                {
                    return (entry._entryType == EntryType.Removed);
                }
            }
            return false;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected internal void BaseRemove(object key)
        {
            this.BaseRemove(key, false);
        }

        private void BaseRemove(object key, bool throwIfMissing)
        {
            if (this.IsReadOnly())
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_read_only"));
            }
            int index = 0;
            bool flag = false;
            foreach (Entry entry in this._items)
            {
                if (!this.CompareKeys(key, entry.GetKey(this)))
                {
                    goto Label_01B8;
                }
                flag = true;
                if (entry._value == null)
                {
                    if (throwIfMissing)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_entry_not_found", new object[] { key }));
                    }
                    return;
                }
                if (entry._value.LockItem)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_attribute_locked", new object[] { key }));
                }
                if (!entry._value.ElementPresent)
                {
                    base.CheckLockedElement(this._removeElement, null);
                }
                switch (entry._entryType)
                {
                    case EntryType.Removed:
                        if (throwIfMissing)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_entry_already_removed"));
                        }
                        goto Label_01AC;

                    case EntryType.Added:
                        if ((this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap) && (this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
                        {
                            if ((this.CollectionType == ConfigurationElementCollectionType.BasicMapAlternate) && (index >= (this.Count - this._inheritedCount)))
                            {
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_cannot_remove_inherited_items"));
                            }
                            break;
                        }
                        goto Label_014B;

                    default:
                        if ((this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap) && (this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_elements_may_not_be_removed"));
                        }
                        entry._entryType = EntryType.Removed;
                        this._removedItemCount++;
                        goto Label_01AC;
                }
                if ((this.CollectionType == ConfigurationElementCollectionType.BasicMap) && (index < this._inheritedCount))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_cannot_remove_inherited_items"));
                }
                this._items.RemoveAt(index);
                goto Label_01AC;
            Label_014B:
                entry._entryType = EntryType.Removed;
                this._removedItemCount++;
            Label_01AC:
                this.bModified = true;
                return;
            Label_01B8:
                index++;
            }
            if (!flag)
            {
                if (throwIfMissing)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_entry_not_found", new object[] { key }));
                }
                if ((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap) || (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
                {
                    if (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate)
                    {
                        this._items.Insert((this.Count + this._removedItemCount) - this._inheritedCount, new Entry(EntryType.Removed, key, null));
                    }
                    else
                    {
                        this._items.Add(new Entry(EntryType.Removed, key, null));
                    }
                    this._removedItemCount++;
                }
            }
        }

        protected internal void BaseRemoveAt(int index)
        {
            if (this.IsReadOnly())
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_read_only"));
            }
            int num = 0;
            Entry entry = null;
            foreach (Entry entry2 in this._items)
            {
                if ((num == index) && (entry2._entryType != EntryType.Removed))
                {
                    entry = entry2;
                    break;
                }
                if (entry2._entryType != EntryType.Removed)
                {
                    num++;
                }
            }
            if (entry == null)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("IndexOutOfRange", new object[] { index }));
            }
            if (entry._value.LockItem)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_attribute_locked", new object[] { entry.GetKey(this) }));
            }
            if (!entry._value.ElementPresent)
            {
                base.CheckLockedElement(this._removeElement, null);
            }
            switch (entry._entryType)
            {
                case EntryType.Removed:
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_entry_already_removed"));

                case EntryType.Added:
                    if ((this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap) && (this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
                    {
                        if ((this.CollectionType == ConfigurationElementCollectionType.BasicMapAlternate) && (index >= (this.Count - this._inheritedCount)))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_cannot_remove_inherited_items"));
                        }
                        if ((this.CollectionType == ConfigurationElementCollectionType.BasicMap) && (index < this._inheritedCount))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_cannot_remove_inherited_items"));
                        }
                        this._items.RemoveAt(index);
                        break;
                    }
                    if (!entry._value.ElementPresent)
                    {
                        base.CheckLockedElement(this._removeElement, null);
                    }
                    entry._entryType = EntryType.Removed;
                    this._removedItemCount++;
                    break;

                default:
                    if ((this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap) && (this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_collection_elements_may_not_be_removed"));
                    }
                    entry._entryType = EntryType.Removed;
                    this._removedItemCount++;
                    break;
            }
            this.bModified = true;
        }

        private ConfigurationElement CallCreateNewElement()
        {
            ConfigurationElement element = this.CreateNewElement();
            element.AssociateContext(base._configRecord);
            element.CallInit();
            return element;
        }

        private ConfigurationElement CallCreateNewElement(string elementName)
        {
            ConfigurationElement element = this.CreateNewElement(elementName);
            element.AssociateContext(base._configRecord);
            element.CallInit();
            return element;
        }

        private bool CompareKeys(object key1, object key2)
        {
            if (this._comparer != null)
            {
                return (this._comparer.Compare(key1, key2) == 0);
            }
            return key1.Equals(key2);
        }

        public void CopyTo(ConfigurationElement[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        protected abstract ConfigurationElement CreateNewElement();
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected virtual ConfigurationElement CreateNewElement(string elementName)
        {
            return this.CreateNewElement();
        }

        public override bool Equals(object compareTo)
        {
            if (!(compareTo.GetType() == base.GetType()))
            {
                return false;
            }
            ConfigurationElementCollection elements = (ConfigurationElementCollection) compareTo;
            if (this.Count != elements.Count)
            {
                return false;
            }
            foreach (Entry entry in this.Items)
            {
                bool flag = false;
                foreach (Entry entry2 in elements.Items)
                {
                    if (object.Equals(entry._value, entry2._value))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    return false;
                }
            }
            return true;
        }

        protected abstract object GetElementKey(ConfigurationElement element);
        internal object GetElementKeyInternal(ConfigurationElement element)
        {
            object elementKey = this.GetElementKey(element);
            if (elementKey == null)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_invalid_element_key"));
            }
            return elementKey;
        }

        internal IEnumerator GetElementsEnumerator()
        {
            return new Enumerator(this._items, this);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IEnumerator GetEnumerator()
        {
            return this.GetEnumeratorImpl();
        }

        internal virtual IEnumerator GetEnumeratorImpl()
        {
            return new Enumerator(this._items, this);
        }

        public override int GetHashCode()
        {
            int num = 0;
            foreach (Entry entry in this.Items)
            {
                ConfigurationElement element = entry._value;
                num ^= element.GetHashCode();
            }
            return num;
        }

        protected virtual bool IsElementName(string elementName)
        {
            return false;
        }

        protected virtual bool IsElementRemovable(ConfigurationElement element)
        {
            return true;
        }

        internal bool IsLockableElement(string elementName)
        {
            if ((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap) || (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
            {
                if (!(elementName == this.AddElementName) && !(elementName == this.RemoveElementName))
                {
                    return (elementName == this.ClearElementName);
                }
                return true;
            }
            if (!(elementName == this.ElementName))
            {
                return this.IsElementName(elementName);
            }
            return true;
        }

        protected internal override bool IsModified()
        {
            if (this.bModified)
            {
                return true;
            }
            if (base.IsModified())
            {
                return true;
            }
            foreach (Entry entry in this._items)
            {
                if ((entry._entryType != EntryType.Removed) && entry._value.IsModified())
                {
                    return true;
                }
            }
            return false;
        }

        public override bool IsReadOnly()
        {
            return this.bReadOnly;
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            bool flag = false;
            if ((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap) || (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
            {
                if (elementName == this._addElement)
                {
                    ConfigurationElement element = this.CallCreateNewElement();
                    element.ResetLockLists(this);
                    element.DeserializeElement(reader, false);
                    this.BaseAdd(element);
                    return true;
                }
                if (elementName == this._removeElement)
                {
                    ConfigurationElement element2 = this.CallCreateNewElement();
                    element2.ResetLockLists(this);
                    element2.DeserializeElement(reader, true);
                    if (this.IsElementRemovable(element2))
                    {
                        this.BaseRemove(this.GetElementKeyInternal(element2), false);
                    }
                    return true;
                }
                if (!(elementName == this._clearElement))
                {
                    return flag;
                }
                if (reader.AttributeCount > 0)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        string name = reader.Name;
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_unrecognized_attribute", new object[] { name }), reader);
                    }
                }
                base.CheckLockedElement(elementName, reader);
                reader.MoveToElement();
                this.BaseClear();
                this.bEmitClearTag = true;
                return true;
            }
            if (elementName == this.ElementName)
            {
                if (BaseConfigurationRecord.IsReservedAttributeName(elementName))
                {
                    throw new ArgumentException(System.Configuration.SR.GetString("Basicmap_item_name_reserved", new object[] { elementName }));
                }
                ConfigurationElement element3 = this.CallCreateNewElement();
                element3.ResetLockLists(this);
                element3.DeserializeElement(reader, false);
                this.BaseAdd(element3);
                return true;
            }
            if (!this.IsElementName(elementName))
            {
                return flag;
            }
            if (BaseConfigurationRecord.IsReservedAttributeName(elementName))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Basicmap_item_name_reserved", new object[] { elementName }));
            }
            ConfigurationElement element4 = this.CallCreateNewElement(elementName);
            element4.ResetLockLists(this);
            element4.DeserializeElement(reader, false);
            this.BaseAdd(-1, element4);
            return true;
        }

        internal int RealIndexOf(ConfigurationElement element)
        {
            int num = 0;
            object elementKeyInternal = this.GetElementKeyInternal(element);
            foreach (Entry entry in this._items)
            {
                if (this.CompareKeys(elementKeyInternal, entry.GetKey(this)))
                {
                    return num;
                }
                num++;
            }
            return -1;
        }

        protected internal override void Reset(ConfigurationElement parentElement)
        {
            ConfigurationElementCollection elements = parentElement as ConfigurationElementCollection;
            base.ResetLockLists(parentElement);
            if (elements != null)
            {
                foreach (Entry entry in elements.Items)
                {
                    ConfigurationElement element = this.CallCreateNewElement(entry.GetKey(this).ToString());
                    element.Reset(entry._value);
                    if (((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap) || (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate)) && ((entry._entryType == EntryType.Added) || (entry._entryType == EntryType.Replaced)))
                    {
                        this.BaseAdd(element, true, true);
                    }
                    else if ((this.CollectionType == ConfigurationElementCollectionType.BasicMap) || (this.CollectionType == ConfigurationElementCollectionType.BasicMapAlternate))
                    {
                        this.BaseAdd(-1, element, true);
                    }
                }
                this._inheritedCount = this.Count;
            }
        }

        protected internal override void ResetModified()
        {
            this.bModified = false;
            base.ResetModified();
            foreach (Entry entry in this._items)
            {
                if (entry._entryType != EntryType.Removed)
                {
                    entry._value.ResetModified();
                }
            }
        }

        protected internal override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
        {
            ConfigurationElementCollectionType collectionType = this.CollectionType;
            bool flag = false;
            flag |= base.SerializeElement(writer, serializeCollectionKey);
            if (((collectionType == ConfigurationElementCollectionType.AddRemoveClearMap) || (collectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate)) && (this.bEmitClearTag && (this._clearElement.Length != 0)))
            {
                if (writer != null)
                {
                    writer.WriteStartElement(this._clearElement);
                    writer.WriteEndElement();
                }
                flag = true;
            }
            foreach (Entry entry in this._items)
            {
                switch (collectionType)
                {
                    case ConfigurationElementCollectionType.BasicMap:
                    case ConfigurationElementCollectionType.BasicMapAlternate:
                        if ((entry._entryType == EntryType.Added) || (entry._entryType == EntryType.Replaced))
                        {
                            if ((this.ElementName != null) && (this.ElementName.Length != 0))
                            {
                                if (BaseConfigurationRecord.IsReservedAttributeName(this.ElementName))
                                {
                                    throw new ArgumentException(System.Configuration.SR.GetString("Basicmap_item_name_reserved", new object[] { this.ElementName }));
                                }
                                flag |= entry._value.SerializeToXmlElement(writer, this.ElementName);
                            }
                            else
                            {
                                flag |= entry._value.SerializeElement(writer, false);
                            }
                        }
                        break;

                    case ConfigurationElementCollectionType.AddRemoveClearMap:
                    case ConfigurationElementCollectionType.AddRemoveClearMapAlternate:
                        if (((entry._entryType == EntryType.Removed) || (entry._entryType == EntryType.Replaced)) && (entry._value != null))
                        {
                            if (writer != null)
                            {
                                writer.WriteStartElement(this._removeElement);
                            }
                            flag |= entry._value.SerializeElement(writer, true);
                            if (writer != null)
                            {
                                writer.WriteEndElement();
                            }
                            flag = true;
                        }
                        if ((entry._entryType == EntryType.Added) || (entry._entryType == EntryType.Replaced))
                        {
                            flag |= entry._value.SerializeToXmlElement(writer, this._addElement);
                        }
                        break;
                }
            }
            return flag;
        }

        protected internal override void SetReadOnly()
        {
            this.bReadOnly = true;
            foreach (Entry entry in this._items)
            {
                if (entry._entryType != EntryType.Removed)
                {
                    entry._value.SetReadOnly();
                }
            }
        }

        void ICollection.CopyTo(Array arr, int index)
        {
            foreach (Entry entry in this._items)
            {
                if (entry._entryType != EntryType.Removed)
                {
                    arr.SetValue(entry._value, index++);
                }
            }
        }

        protected internal override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            base.Unmerge(sourceElement, parentElement, saveMode);
            if (sourceElement != null)
            {
                ConfigurationElementCollection elements = parentElement as ConfigurationElementCollection;
                ConfigurationElementCollection elements2 = sourceElement as ConfigurationElementCollection;
                Hashtable hashtable = new Hashtable();
                base._lockedAllExceptAttributesList = sourceElement._lockedAllExceptAttributesList;
                base._lockedAllExceptElementsList = sourceElement._lockedAllExceptElementsList;
                base._fItemLocked = sourceElement._fItemLocked;
                base._lockedAttributesList = sourceElement._lockedAttributesList;
                base._lockedElementsList = sourceElement._lockedElementsList;
                this.AssociateContext(sourceElement._configRecord);
                if (parentElement != null)
                {
                    if (parentElement._lockedAttributesList != null)
                    {
                        base._lockedAttributesList = base.UnMergeLockList(sourceElement._lockedAttributesList, parentElement._lockedAttributesList, saveMode);
                    }
                    if (parentElement._lockedElementsList != null)
                    {
                        base._lockedElementsList = base.UnMergeLockList(sourceElement._lockedElementsList, parentElement._lockedElementsList, saveMode);
                    }
                    if (parentElement._lockedAllExceptAttributesList != null)
                    {
                        base._lockedAllExceptAttributesList = base.UnMergeLockList(sourceElement._lockedAllExceptAttributesList, parentElement._lockedAllExceptAttributesList, saveMode);
                    }
                    if (parentElement._lockedAllExceptElementsList != null)
                    {
                        base._lockedAllExceptElementsList = base.UnMergeLockList(sourceElement._lockedAllExceptElementsList, parentElement._lockedAllExceptElementsList, saveMode);
                    }
                }
                if ((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap) || (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
                {
                    this.bCollectionCleared = elements2.bCollectionCleared;
                    this.EmitClear = (((saveMode == ConfigurationSaveMode.Full) && (this._clearElement.Length != 0)) || ((saveMode == ConfigurationSaveMode.Modified) && this.bCollectionCleared)) || elements2.EmitClear;
                    if ((elements != null) && !this.EmitClear)
                    {
                        foreach (Entry entry in elements.Items)
                        {
                            if (entry._entryType != EntryType.Removed)
                            {
                                hashtable[entry.GetKey(this)] = InheritedType.inParent;
                            }
                        }
                    }
                    foreach (Entry entry2 in elements2.Items)
                    {
                        if (entry2._entryType != EntryType.Removed)
                        {
                            if (hashtable.Contains(entry2.GetKey(this)))
                            {
                                Entry entry3 = (Entry) elements.Items[elements.RealIndexOf(entry2._value)];
                                ConfigurationElement element = entry2._value;
                                if (element.Equals(entry3._value))
                                {
                                    hashtable[entry2.GetKey(this)] = InheritedType.inBothSame;
                                    if (saveMode == ConfigurationSaveMode.Modified)
                                    {
                                        if (element.IsModified())
                                        {
                                            hashtable[entry2.GetKey(this)] = InheritedType.inBothDiff;
                                        }
                                        else if (element.ElementPresent)
                                        {
                                            hashtable[entry2.GetKey(this)] = InheritedType.inBothCopyNoRemove;
                                        }
                                    }
                                }
                                else
                                {
                                    hashtable[entry2.GetKey(this)] = InheritedType.inBothDiff;
                                    if ((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) && (entry2._entryType == EntryType.Added))
                                    {
                                        hashtable[entry2.GetKey(this)] = InheritedType.inBothCopyNoRemove;
                                    }
                                }
                            }
                            else
                            {
                                hashtable[entry2.GetKey(this)] = InheritedType.inSelf;
                            }
                        }
                    }
                    if ((elements != null) && !this.EmitClear)
                    {
                        foreach (Entry entry4 in elements.Items)
                        {
                            if (entry4._entryType != EntryType.Removed)
                            {
                                switch (((InheritedType) hashtable[entry4.GetKey(this)]))
                                {
                                    case InheritedType.inParent:
                                    case InheritedType.inBothDiff:
                                    {
                                        ConfigurationElement element2 = this.CallCreateNewElement(entry4.GetKey(this).ToString());
                                        element2.Reset(entry4._value);
                                        this.BaseAdd(element2, this.ThrowOnDuplicate, true);
                                        this.BaseRemove(entry4.GetKey(this), false);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    foreach (Entry entry5 in elements2.Items)
                    {
                        if (entry5._entryType != EntryType.Removed)
                        {
                            InheritedType type2 = (InheritedType) hashtable[entry5.GetKey(this)];
                            switch (type2)
                            {
                                case InheritedType.inSelf:
                                case InheritedType.inBothDiff:
                                case InheritedType.inBothCopyNoRemove:
                                {
                                    ConfigurationElement element3 = this.CallCreateNewElement(entry5.GetKey(this).ToString());
                                    element3.Unmerge(entry5._value, null, saveMode);
                                    if (type2 == InheritedType.inSelf)
                                    {
                                        element3.RemoveAllInheritedLocks();
                                    }
                                    this.BaseAdd(element3, this.ThrowOnDuplicate, true);
                                    break;
                                }
                            }
                        }
                    }
                }
                else if ((this.CollectionType == ConfigurationElementCollectionType.BasicMap) || (this.CollectionType == ConfigurationElementCollectionType.BasicMapAlternate))
                {
                    foreach (Entry entry6 in elements2.Items)
                    {
                        bool flag = false;
                        Entry entry7 = null;
                        if ((entry6._entryType == EntryType.Added) || (entry6._entryType == EntryType.Replaced))
                        {
                            bool flag2 = false;
                            if (elements != null)
                            {
                                foreach (Entry entry8 in elements.Items)
                                {
                                    if (object.Equals(entry6.GetKey(this), entry8.GetKey(this)) && !this.IsElementName(entry6.GetKey(this).ToString()))
                                    {
                                        flag = true;
                                        entry7 = entry8;
                                    }
                                    if (object.Equals(entry6._value, entry8._value))
                                    {
                                        flag = true;
                                        flag2 = true;
                                        entry7 = entry8;
                                        break;
                                    }
                                }
                            }
                            ConfigurationElement element4 = this.CallCreateNewElement(entry6.GetKey(this).ToString());
                            if (!flag)
                            {
                                element4.Unmerge(entry6._value, null, saveMode);
                                this.BaseAdd(-1, element4, true);
                            }
                            else
                            {
                                ConfigurationElement element5 = entry6._value;
                                if ((!flag2 || ((saveMode == ConfigurationSaveMode.Modified) && element5.IsModified())) || (saveMode == ConfigurationSaveMode.Full))
                                {
                                    element4.Unmerge(entry6._value, entry7._value, saveMode);
                                    this.BaseAdd(-1, element4, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected internal string AddElementName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._addElement;
            }
            set
            {
                this._addElement = value;
                if (BaseConfigurationRecord.IsReservedAttributeName(value))
                {
                    throw new ArgumentException(System.Configuration.SR.GetString("Item_name_reserved", new object[] { "add", value }));
                }
            }
        }

        protected internal string ClearElementName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._clearElement;
            }
            set
            {
                if (BaseConfigurationRecord.IsReservedAttributeName(value))
                {
                    throw new ArgumentException(System.Configuration.SR.GetString("Item_name_reserved", new object[] { "clear", value }));
                }
                this._clearElement = value;
            }
        }

        public virtual ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        public int Count
        {
            get
            {
                return (this._items.Count - this._removedItemCount);
            }
        }

        protected virtual string ElementName
        {
            get
            {
                return "";
            }
        }

        public bool EmitClear
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.bEmitClearTag;
            }
            set
            {
                if (this.IsReadOnly())
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_base_read_only"));
                }
                if (value)
                {
                    base.CheckLockedElement(this._clearElement, null);
                    base.CheckLockedElement(this._removeElement, null);
                }
                this.bModified = true;
                this.bEmitClearTag = value;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        private ArrayList Items
        {
            get
            {
                return this._items;
            }
        }

        internal string LockableElements
        {
            get
            {
                if ((this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap) || (this.CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
                {
                    string str = "'" + this.AddElementName + "'";
                    if (this.RemoveElementName.Length != 0)
                    {
                        str = str + ", '" + this.RemoveElementName + "'";
                    }
                    if (this.ClearElementName.Length != 0)
                    {
                        str = str + ", '" + this.ClearElementName + "'";
                    }
                    return str;
                }
                if (!string.IsNullOrEmpty(this.ElementName))
                {
                    return ("'" + this.ElementName + "'");
                }
                return string.Empty;
            }
        }

        protected internal string RemoveElementName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._removeElement;
            }
            set
            {
                if (BaseConfigurationRecord.IsReservedAttributeName(value))
                {
                    throw new ArgumentException(System.Configuration.SR.GetString("Item_name_reserved", new object[] { "remove", value }));
                }
                this._removeElement = value;
            }
        }

        public object SyncRoot
        {
            get
            {
                return null;
            }
        }

        protected virtual bool ThrowOnDuplicate
        {
            get
            {
                if ((this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap) && (this.CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate))
                {
                    return false;
                }
                return true;
            }
        }

        private class Entry
        {
            internal ConfigurationElementCollection.EntryType _entryType;
            internal object _key;
            internal ConfigurationElement _value;

            internal Entry(ConfigurationElementCollection.EntryType type, object key, ConfigurationElement value)
            {
                this._entryType = type;
                this._key = key;
                this._value = value;
            }

            internal object GetKey(ConfigurationElementCollection ThisCollection)
            {
                if (this._value != null)
                {
                    return ThisCollection.GetElementKeyInternal(this._value);
                }
                return this._key;
            }
        }

        private enum EntryType
        {
            Inherited,
            Replaced,
            Removed,
            Added
        }

        private class Enumerator : IDictionaryEnumerator, IEnumerator
        {
            private DictionaryEntry _current = new DictionaryEntry();
            private IEnumerator _itemsEnumerator;
            private ConfigurationElementCollection ThisCollection;

            internal Enumerator(ArrayList items, ConfigurationElementCollection collection)
            {
                this._itemsEnumerator = items.GetEnumerator();
                this.ThisCollection = collection;
            }

            bool IEnumerator.MoveNext()
            {
                while (this._itemsEnumerator.MoveNext())
                {
                    ConfigurationElementCollection.Entry current = (ConfigurationElementCollection.Entry) this._itemsEnumerator.Current;
                    if (current._entryType != ConfigurationElementCollection.EntryType.Removed)
                    {
                        this._current.Key = (current.GetKey(this.ThisCollection) != null) ? current.GetKey(this.ThisCollection) : "key";
                        this._current.Value = current._value;
                        return true;
                    }
                }
                return false;
            }

            void IEnumerator.Reset()
            {
                this._itemsEnumerator.Reset();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    return this._current;
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    return this._current.Key;
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    return this._current.Value;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this._current.Value;
                }
            }
        }

        private enum InheritedType
        {
            inNeither,
            inParent,
            inSelf,
            inBothSame,
            inBothDiff,
            inBothCopyNoRemove
        }
    }
}

