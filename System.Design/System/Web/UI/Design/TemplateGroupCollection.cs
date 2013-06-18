namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Design;
    using System.Globalization;
    using System.Reflection;

    public sealed class TemplateGroupCollection : IList, ICollection, IEnumerable
    {
        private ArrayList _list;

        public TemplateGroupCollection()
        {
        }

        internal TemplateGroupCollection(TemplateGroup[] verbs)
        {
            for (int i = 0; i < verbs.Length; i++)
            {
                this.Add(verbs[i]);
            }
        }

        public int Add(TemplateGroup group)
        {
            return this.InternalList.Add(group);
        }

        public void AddRange(TemplateGroupCollection groups)
        {
            this.InternalList.AddRange(groups);
        }

        public void Clear()
        {
            this.InternalList.Clear();
        }

        public bool Contains(TemplateGroup group)
        {
            return this.InternalList.Contains(group);
        }

        public void CopyTo(TemplateGroup[] array, int index)
        {
            this.InternalList.CopyTo(array, index);
        }

        public int IndexOf(TemplateGroup group)
        {
            return this.InternalList.IndexOf(group);
        }

        public void Insert(int index, TemplateGroup group)
        {
            this.InternalList.Insert(index, group);
        }

        public void Remove(TemplateGroup group)
        {
            this.InternalList.Remove(group);
        }

        public void RemoveAt(int index)
        {
            this.InternalList.RemoveAt(index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.InternalList.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InternalList.GetEnumerator();
        }

        int IList.Add(object o)
        {
            if (!(o is TemplateGroup))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "TemplateGroup" }), "o");
            }
            return this.Add((TemplateGroup) o);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object o)
        {
            if (!(o is TemplateGroup))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "TemplateGroup" }), "o");
            }
            return this.Contains((TemplateGroup) o);
        }

        int IList.IndexOf(object o)
        {
            if (!(o is TemplateGroup))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "TemplateGroup" }), "o");
            }
            return this.IndexOf((TemplateGroup) o);
        }

        void IList.Insert(int index, object o)
        {
            if (!(o is TemplateGroup))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "TemplateGroup" }), "o");
            }
            this.Insert(index, (TemplateGroup) o);
        }

        void IList.Remove(object o)
        {
            if (!(o is TemplateGroup))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "TemplateGroup" }), "o");
            }
            this.Remove((TemplateGroup) o);
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

        public TemplateGroup this[int index]
        {
            get
            {
                return (TemplateGroup) this.InternalList[index];
            }
            set
            {
                this.InternalList[index] = value;
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
                return this.InternalList.IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this.InternalList.SyncRoot;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return this.InternalList.IsFixedSize;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this.InternalList.IsReadOnly;
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
                if (!(value is TemplateGroup))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, System.Design.SR.GetString("WrongType"), new object[] { "TemplateGroup" }), "value");
                }
                this[index] = (TemplateGroup) value;
            }
        }
    }
}

