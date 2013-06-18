namespace System.Data.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal abstract class DataSourceCollectionBase : CollectionBase, INamedObjectCollection, ICollection, IEnumerable, IObjectWithParent
    {
        private DataSourceComponent collectionHost;

        internal DataSourceCollectionBase(DataSourceComponent collectionHost)
        {
            this.collectionHost = collectionHost;
        }

        protected virtual string CreateUniqueName(IDataSourceNamedObject value)
        {
            string proposedNameRoot = StringUtil.NotEmpty(value.Name) ? value.Name : value.PublicTypeName;
            return this.NameService.CreateUniqueName(this, proposedNameRoot, 1);
        }

        protected internal virtual void EnsureUniqueName(IDataSourceNamedObject namedObject)
        {
            if (((namedObject.Name == null) || (namedObject.Name.Length == 0)) || (this.FindObject(namedObject.Name) != null))
            {
                namedObject.Name = this.CreateUniqueName(namedObject);
            }
        }

        protected internal virtual IDataSourceNamedObject FindObject(string name)
        {
            IEnumerator enumerator = base.InnerList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                IDataSourceNamedObject current = (IDataSourceNamedObject) enumerator.Current;
                if (StringUtil.EqualValue(current.Name, name))
                {
                    return current;
                }
            }
            return null;
        }

        public INameService GetNameService()
        {
            return this.NameService;
        }

        public void InsertBefore(object value, object refObject)
        {
            int index = base.List.IndexOf(refObject);
            if (index >= 0)
            {
                base.List.Insert(index, value);
            }
            else
            {
                base.List.Add(value);
            }
        }

        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            this.ValidateType(value);
        }

        public void Remove(string name)
        {
            INamedObject obj2 = NamedObjectUtil.Find(this, name);
            if (obj2 != null)
            {
                base.List.Remove(obj2);
            }
        }

        protected internal virtual void ValidateName(IDataSourceNamedObject obj)
        {
            this.NameService.ValidateName(obj.Name);
        }

        protected void ValidateType(object value)
        {
            if (!this.ItemType.IsInstanceOfType(value))
            {
                throw new InternalException("{0} can hold only {1} objects", 0x4e30, true);
            }
        }

        protected internal virtual void ValidateUniqueName(IDataSourceNamedObject obj, string proposedName)
        {
            this.NameService.ValidateUniqueName(this, obj, proposedName);
        }

        internal virtual DataSourceComponent CollectionHost
        {
            get
            {
                return this.collectionHost;
            }
            set
            {
                this.collectionHost = value;
            }
        }

        protected virtual Type ItemType
        {
            get
            {
                return typeof(IDataSourceNamedObject);
            }
        }

        protected abstract INameService NameService { get; }

        [Browsable(false)]
        object IObjectWithParent.Parent
        {
            get
            {
                return this.collectionHost;
            }
        }
    }
}

