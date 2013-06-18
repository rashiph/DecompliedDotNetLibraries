namespace System.Web
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class SiteMapNodeCollection : IHierarchicalEnumerable, IList, ICollection, IEnumerable
    {
        private int _initialSize;
        private ArrayList _innerList;
        internal static SiteMapNodeCollection Empty = new ReadOnlySiteMapNodeCollection(new SiteMapNodeCollection());

        public SiteMapNodeCollection()
        {
            this._initialSize = 10;
        }

        public SiteMapNodeCollection(int capacity)
        {
            this._initialSize = 10;
            this._initialSize = capacity;
        }

        public SiteMapNodeCollection(SiteMapNode value)
        {
            this._initialSize = 10;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this._initialSize = 1;
            this.List.Add(value);
        }

        public SiteMapNodeCollection(SiteMapNode[] value)
        {
            this._initialSize = 10;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this._initialSize = value.Length;
            this.AddRangeInternal(value);
        }

        public SiteMapNodeCollection(SiteMapNodeCollection value)
        {
            this._initialSize = 10;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this._initialSize = value.Count;
            this.AddRangeInternal(value);
        }

        public virtual int Add(SiteMapNode value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return this.List.Add(value);
        }

        public virtual void AddRange(SiteMapNode[] value)
        {
            this.AddRangeInternal(value);
        }

        public virtual void AddRange(SiteMapNodeCollection value)
        {
            this.AddRangeInternal(value);
        }

        private void AddRangeInternal(IList value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.List.AddRange(value);
        }

        public virtual void Clear()
        {
            this.List.Clear();
        }

        public virtual bool Contains(SiteMapNode value)
        {
            return this.List.Contains(value);
        }

        public virtual void CopyTo(SiteMapNode[] array, int index)
        {
            this.CopyToInternal(array, index);
        }

        internal virtual void CopyToInternal(Array array, int index)
        {
            this.List.CopyTo(array, index);
        }

        public SiteMapDataSourceView GetDataSourceView(SiteMapDataSource owner, string viewName)
        {
            return new SiteMapDataSourceView(owner, viewName, this);
        }

        public virtual IEnumerator GetEnumerator()
        {
            return this.List.GetEnumerator();
        }

        public SiteMapHierarchicalDataSourceView GetHierarchicalDataSourceView()
        {
            return new SiteMapHierarchicalDataSourceView(this);
        }

        public virtual IHierarchyData GetHierarchyData(object enumeratedItem)
        {
            return (enumeratedItem as IHierarchyData);
        }

        public virtual int IndexOf(SiteMapNode value)
        {
            return this.List.IndexOf(value);
        }

        public virtual void Insert(int index, SiteMapNode value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.List.Insert(index, value);
        }

        protected virtual void OnValidate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(value is SiteMapNode))
            {
                throw new ArgumentException(System.Web.SR.GetString("SiteMapNodeCollection_Invalid_Type", new object[] { value.GetType().ToString() }));
            }
        }

        public static SiteMapNodeCollection ReadOnly(SiteMapNodeCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            return new ReadOnlySiteMapNodeCollection(collection);
        }

        public virtual void Remove(SiteMapNode value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.List.Remove(value);
        }

        public virtual void RemoveAt(int index)
        {
            this.List.RemoveAt(index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyToInternal(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        int IList.Add(object value)
        {
            this.OnValidate(value);
            return this.Add((SiteMapNode) value);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            this.OnValidate(value);
            return this.Contains((SiteMapNode) value);
        }

        int IList.IndexOf(object value)
        {
            this.OnValidate(value);
            return this.IndexOf((SiteMapNode) value);
        }

        void IList.Insert(int index, object value)
        {
            this.OnValidate(value);
            this.Insert(index, (SiteMapNode) value);
        }

        void IList.Remove(object value)
        {
            this.OnValidate(value);
            this.Remove((SiteMapNode) value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        IHierarchyData IHierarchicalEnumerable.GetHierarchyData(object enumeratedItem)
        {
            return this.GetHierarchyData(enumeratedItem);
        }

        public virtual int Count
        {
            get
            {
                return this.List.Count;
            }
        }

        public virtual bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return this.List.IsSynchronized;
            }
        }

        public virtual SiteMapNode this[int index]
        {
            get
            {
                return (SiteMapNode) this.List[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.List[index] = value;
            }
        }

        private ArrayList List
        {
            get
            {
                if (this._innerList == null)
                {
                    this._innerList = new ArrayList(this._initialSize);
                }
                return this._innerList;
            }
        }

        public virtual object SyncRoot
        {
            get
            {
                return this.List.SyncRoot;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return this.IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this.SyncRoot;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return this.IsFixedSize;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this.OnValidate(value);
                this[index] = (SiteMapNode) value;
            }
        }

        private sealed class ReadOnlySiteMapNodeCollection : SiteMapNodeCollection
        {
            private SiteMapNodeCollection _internalCollection;

            internal ReadOnlySiteMapNodeCollection(SiteMapNodeCollection collection)
            {
                if (collection == null)
                {
                    throw new ArgumentNullException("collection");
                }
                this._internalCollection = collection;
            }

            public override int Add(SiteMapNode value)
            {
                throw new NotSupportedException(System.Web.SR.GetString("Collection_readonly"));
            }

            public override void AddRange(SiteMapNode[] value)
            {
                throw new NotSupportedException(System.Web.SR.GetString("Collection_readonly"));
            }

            public override void AddRange(SiteMapNodeCollection value)
            {
                throw new NotSupportedException(System.Web.SR.GetString("Collection_readonly"));
            }

            public override void Clear()
            {
                throw new NotSupportedException(System.Web.SR.GetString("Collection_readonly"));
            }

            public override bool Contains(SiteMapNode node)
            {
                return this._internalCollection.Contains(node);
            }

            internal override void CopyToInternal(Array array, int index)
            {
                this._internalCollection.List.CopyTo(array, index);
            }

            public override IEnumerator GetEnumerator()
            {
                return this._internalCollection.GetEnumerator();
            }

            public override int IndexOf(SiteMapNode value)
            {
                return this._internalCollection.IndexOf(value);
            }

            public override void Insert(int index, SiteMapNode value)
            {
                throw new NotSupportedException(System.Web.SR.GetString("Collection_readonly"));
            }

            public override void Remove(SiteMapNode value)
            {
                throw new NotSupportedException(System.Web.SR.GetString("Collection_readonly"));
            }

            public override void RemoveAt(int index)
            {
                throw new NotSupportedException(System.Web.SR.GetString("Collection_readonly"));
            }

            public override int Count
            {
                get
                {
                    return this._internalCollection.Count;
                }
            }

            public override bool IsFixedSize
            {
                get
                {
                    return true;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public override bool IsSynchronized
            {
                get
                {
                    return this._internalCollection.IsSynchronized;
                }
            }

            public override SiteMapNode this[int index]
            {
                get
                {
                    return this._internalCollection[index];
                }
                set
                {
                    throw new NotSupportedException(System.Web.SR.GetString("Collection_readonly"));
                }
            }

            public override object SyncRoot
            {
                get
                {
                    return this._internalCollection.SyncRoot;
                }
            }
        }
    }
}

