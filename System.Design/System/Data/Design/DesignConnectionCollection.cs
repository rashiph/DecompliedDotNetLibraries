namespace System.Data.Design
{
    using System;
    using System.Collections;

    internal class DesignConnectionCollection : DataSourceCollectionBase, IDesignConnectionCollection, INamedObjectCollection, ICollection, IEnumerable
    {
        internal DesignConnectionCollection(DataSourceComponent collectionHost) : base(collectionHost)
        {
        }

        public int Add(IDesignConnection connection)
        {
            return base.List.Add(connection);
        }

        public bool Contains(IDesignConnection connection)
        {
            return base.List.Contains(connection);
        }

        public IDesignConnection Get(string name)
        {
            return (IDesignConnection) NamedObjectUtil.Find(this, name);
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            base.OnSet(index, oldValue, newValue);
            base.ValidateType(newValue);
            IDesignConnection connection = (IDesignConnection) oldValue;
            IDesignConnection connection2 = (IDesignConnection) newValue;
            if (!StringUtil.EqualValue(connection.Name, connection2.Name))
            {
                this.ValidateUniqueName(connection2, connection2.Name);
            }
        }

        public void Remove(IDesignConnection connection)
        {
            base.List.Remove(connection);
        }

        public void Set(IDesignConnection connection)
        {
            INamedObject obj2 = NamedObjectUtil.Find(this, connection.Name);
            if (obj2 != null)
            {
                base.List.Remove(obj2);
            }
            base.List.Add(connection);
        }

        protected override Type ItemType
        {
            get
            {
                return typeof(IDesignConnection);
            }
        }

        protected override INameService NameService
        {
            get
            {
                return SimpleNameService.DefaultInstance;
            }
        }
    }
}

