namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Windows.Forms.Layout;

    [Editor("System.Windows.Forms.Design.StyleCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public abstract class TableLayoutStyleCollection : IList, ICollection, IEnumerable
    {
        private ArrayList _innerList = new ArrayList();
        private IArrangedElement _owner;

        internal TableLayoutStyleCollection(IArrangedElement owner)
        {
            this._owner = owner;
        }

        public int Add(TableLayoutStyle style)
        {
            return ((IList) this).Add(style);
        }

        public void Clear()
        {
            foreach (TableLayoutStyle style in this._innerList)
            {
                style.Owner = null;
            }
            this._innerList.Clear();
            this.PerformLayoutIfOwned();
        }

        private void EnsureNotOwned(TableLayoutStyle style)
        {
            if (style.Owner != null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("OnlyOneControl", new object[] { style.GetType().Name }), "style");
            }
        }

        internal void EnsureOwnership(IArrangedElement owner)
        {
            this._owner = owner;
            for (int i = 0; i < this.Count; i++)
            {
                this[i].Owner = owner;
            }
        }

        private void PerformLayoutIfOwned()
        {
            if (this.Owner != null)
            {
                LayoutTransaction.DoLayout(this.Owner, this.Owner, this.PropertyName);
            }
        }

        public void RemoveAt(int index)
        {
            TableLayoutStyle style = (TableLayoutStyle) this._innerList[index];
            style.Owner = null;
            this._innerList.RemoveAt(index);
            this.PerformLayoutIfOwned();
        }

        void ICollection.CopyTo(Array array, int startIndex)
        {
            this._innerList.CopyTo(array, startIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._innerList.GetEnumerator();
        }

        int IList.Add(object style)
        {
            this.EnsureNotOwned((TableLayoutStyle) style);
            ((TableLayoutStyle) style).Owner = this.Owner;
            int num = this._innerList.Add(style);
            this.PerformLayoutIfOwned();
            return num;
        }

        bool IList.Contains(object style)
        {
            return this._innerList.Contains(style);
        }

        int IList.IndexOf(object style)
        {
            return this._innerList.IndexOf(style);
        }

        void IList.Insert(int index, object style)
        {
            this.EnsureNotOwned((TableLayoutStyle) style);
            ((TableLayoutStyle) style).Owner = this.Owner;
            this._innerList.Insert(index, style);
            this.PerformLayoutIfOwned();
        }

        void IList.Remove(object style)
        {
            ((TableLayoutStyle) style).Owner = null;
            this._innerList.Remove(style);
            this.PerformLayoutIfOwned();
        }

        public int Count
        {
            get
            {
                return this._innerList.Count;
            }
        }

        public TableLayoutStyle this[int index]
        {
            get
            {
                return (TableLayoutStyle) ((IList) this)[index];
            }
            set
            {
                ((IList) this)[index] = value;
            }
        }

        internal IArrangedElement Owner
        {
            get
            {
                return this._owner;
            }
        }

        internal virtual string PropertyName
        {
            get
            {
                return null;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return this._innerList.IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this._innerList.SyncRoot;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return this._innerList.IsFixedSize;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this._innerList.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this._innerList[index];
            }
            set
            {
                TableLayoutStyle style = (TableLayoutStyle) value;
                this.EnsureNotOwned(style);
                style.Owner = this.Owner;
                this._innerList[index] = style;
                this.PerformLayoutIfOwned();
            }
        }
    }
}

