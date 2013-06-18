namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Design;
    using System.Globalization;
    using System.Reflection;

    public class DesignerRegionCollection : IList, ICollection, IEnumerable
    {
        private ArrayList _list;
        private ControlDesigner _owner;

        public DesignerRegionCollection()
        {
        }

        public DesignerRegionCollection(ControlDesigner owner)
        {
            this._owner = owner;
        }

        public int Add(DesignerRegion region)
        {
            return this.InternalList.Add(region);
        }

        public void Clear()
        {
            this.InternalList.Clear();
        }

        public bool Contains(DesignerRegion region)
        {
            return this.InternalList.Contains(region);
        }

        public void CopyTo(Array array, int index)
        {
            this.InternalList.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.InternalList.GetEnumerator();
        }

        public int IndexOf(DesignerRegion region)
        {
            return this.InternalList.IndexOf(region);
        }

        public void Insert(int index, DesignerRegion region)
        {
            this.InternalList.Insert(index, region);
        }

        public void Remove(DesignerRegion region)
        {
            this.InternalList.Remove(region);
        }

        public void RemoveAt(int index)
        {
            this.InternalList.RemoveAt(index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        int IList.Add(object o)
        {
            if (!(o is DesignerRegion))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "DesignerRegion" }), "o");
            }
            return this.Add((DesignerRegion) o);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object o)
        {
            if (!(o is DesignerRegion))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "DesignerRegion" }), "o");
            }
            return this.Contains((DesignerRegion) o);
        }

        int IList.IndexOf(object o)
        {
            if (!(o is DesignerRegion))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "DesignerRegion" }), "o");
            }
            return this.IndexOf((DesignerRegion) o);
        }

        void IList.Insert(int index, object o)
        {
            if (!(o is DesignerRegion))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "DesignerRegion" }), "o");
            }
            this.Insert(index, (DesignerRegion) o);
        }

        void IList.Remove(object o)
        {
            if (!(o is DesignerRegion))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "DesignerRegion" }), "o");
            }
            this.Remove((DesignerRegion) o);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        public int Count
        {
            get
            {
                return this.InternalList.Count;
            }
        }

        private ArrayList InternalList
        {
            get
            {
                if (this._list == null)
                {
                    this._list = new ArrayList();
                }
                return this._list;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return this.InternalList.IsFixedSize;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.InternalList.IsReadOnly;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this.InternalList.IsSynchronized;
            }
        }

        public DesignerRegion this[int index]
        {
            get
            {
                return (DesignerRegion) this.InternalList[index];
            }
            set
            {
                this.InternalList[index] = value;
            }
        }

        public ControlDesigner Owner
        {
            get
            {
                return this._owner;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.InternalList.SyncRoot;
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
                if (!(value is DesignerRegion))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "DesignerRegion" }), "value");
                }
                this[index] = (DesignerRegion) value;
            }
        }
    }
}

