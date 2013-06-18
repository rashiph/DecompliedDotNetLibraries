namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    public class ObjectPersistData
    {
        private ArrayList _allPropertyEntries;
        private IDictionary _builtObjects;
        private ArrayList _collectionItems;
        private ArrayList _eventEntries;
        private bool _isCollection;
        private bool _localize;
        private Type _objectType;
        private IDictionary _propertyTableByFilter;
        private IDictionary _propertyTableByProperty;
        private string _resourceKey;

        public ObjectPersistData(ControlBuilder builder, IDictionary builtObjects)
        {
            this._objectType = builder.ControlType;
            this._localize = builder.Localize;
            this._resourceKey = builder.GetResourceKey();
            this._builtObjects = builtObjects;
            if (typeof(ICollection).IsAssignableFrom(this._objectType))
            {
                this._isCollection = true;
            }
            this._collectionItems = new ArrayList();
            this._propertyTableByFilter = new HybridDictionary(true);
            this._propertyTableByProperty = new HybridDictionary(true);
            this._allPropertyEntries = new ArrayList();
            this._eventEntries = new ArrayList();
            foreach (PropertyEntry entry in builder.SimplePropertyEntries)
            {
                this.AddPropertyEntry(entry);
            }
            foreach (PropertyEntry entry2 in builder.ComplexPropertyEntries)
            {
                this.AddPropertyEntry(entry2);
            }
            foreach (PropertyEntry entry3 in builder.TemplatePropertyEntries)
            {
                this.AddPropertyEntry(entry3);
            }
            foreach (PropertyEntry entry4 in builder.BoundPropertyEntries)
            {
                this.AddPropertyEntry(entry4);
            }
            foreach (EventEntry entry5 in builder.EventEntries)
            {
                this.AddEventEntry(entry5);
            }
        }

        private void AddEventEntry(EventEntry entry)
        {
            this._eventEntries.Add(entry);
        }

        private void AddPropertyEntry(PropertyEntry entry)
        {
            if ((this._isCollection && (entry is ComplexPropertyEntry)) && ((ComplexPropertyEntry) entry).IsCollectionItem)
            {
                this._collectionItems.Add(entry);
            }
            else
            {
                IDictionary dictionary = (IDictionary) this._propertyTableByFilter[entry.Filter];
                if (dictionary == null)
                {
                    dictionary = new HybridDictionary(true);
                    this._propertyTableByFilter[entry.Filter] = dictionary;
                }
                dictionary[entry.Name] = entry;
                ArrayList list = (ArrayList) this._propertyTableByProperty[entry.Name];
                if (list == null)
                {
                    list = new ArrayList();
                    this._propertyTableByProperty[entry.Name] = list;
                }
                list.Add(entry);
            }
            this._allPropertyEntries.Add(entry);
        }

        public void AddToObjectControlBuilderTable(IDictionary table)
        {
            if (this._builtObjects != null)
            {
                foreach (DictionaryEntry entry in this._builtObjects)
                {
                    table[entry.Key] = entry.Value;
                }
            }
        }

        public IDictionary GetFilteredProperties(string filter)
        {
            return (IDictionary) this._propertyTableByFilter[filter];
        }

        public PropertyEntry GetFilteredProperty(string filter, string name)
        {
            IDictionary filteredProperties = this.GetFilteredProperties(filter);
            if (filteredProperties != null)
            {
                return (PropertyEntry) filteredProperties[name];
            }
            return null;
        }

        public ICollection GetPropertyAllFilters(string name)
        {
            ICollection is2 = (ICollection) this._propertyTableByProperty[name];
            if (is2 == null)
            {
                return new ArrayList();
            }
            return is2;
        }

        public ICollection AllPropertyEntries
        {
            get
            {
                return this._allPropertyEntries;
            }
        }

        public IDictionary BuiltObjects
        {
            get
            {
                return this._builtObjects;
            }
        }

        public ICollection CollectionItems
        {
            get
            {
                return this._collectionItems;
            }
        }

        public ICollection EventEntries
        {
            get
            {
                return this._eventEntries;
            }
        }

        public bool IsCollection
        {
            get
            {
                return this._isCollection;
            }
        }

        public bool Localize
        {
            get
            {
                return this._localize;
            }
        }

        public Type ObjectType
        {
            get
            {
                return this._objectType;
            }
        }

        public string ResourceKey
        {
            get
            {
                return this._resourceKey;
            }
        }
    }
}

