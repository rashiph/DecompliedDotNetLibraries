namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Reflection;

    [Editor("System.Web.UI.Design.WebControls.TableRowsCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public sealed class TableRowCollection : IList, ICollection, IEnumerable
    {
        private Table owner;

        internal TableRowCollection(Table owner)
        {
            this.owner = owner;
        }

        public int Add(TableRow row)
        {
            this.AddAt(-1, row);
            return (this.owner.Controls.Count - 1);
        }

        public void AddAt(int index, TableRow row)
        {
            this.owner.Controls.AddAt(index, row);
            if (row.TableSection != TableRowSection.TableBody)
            {
                this.owner.HasRowSections = true;
            }
        }

        public void AddRange(TableRow[] rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException("rows");
            }
            foreach (TableRow row in rows)
            {
                this.Add(row);
            }
        }

        public void Clear()
        {
            if (this.owner.HasControls())
            {
                this.owner.Controls.Clear();
                this.owner.HasRowSections = false;
            }
        }

        public void CopyTo(Array array, int index)
        {
            IEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                array.SetValue(enumerator.Current, index++);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this.owner.Controls.GetEnumerator();
        }

        public int GetRowIndex(TableRow row)
        {
            if (this.owner.HasControls())
            {
                return this.owner.Controls.IndexOf(row);
            }
            return -1;
        }

        public void Remove(TableRow row)
        {
            this.owner.Controls.Remove(row);
        }

        public void RemoveAt(int index)
        {
            this.owner.Controls.RemoveAt(index);
        }

        int IList.Add(object o)
        {
            return this.Add((TableRow) o);
        }

        bool IList.Contains(object o)
        {
            return this.owner.Controls.Contains((TableRow) o);
        }

        int IList.IndexOf(object o)
        {
            return this.owner.Controls.IndexOf((TableRow) o);
        }

        void IList.Insert(int index, object o)
        {
            this.AddAt(index, (TableRow) o);
        }

        void IList.Remove(object o)
        {
            this.Remove((TableRow) o);
        }

        public int Count
        {
            get
            {
                if (this.owner.HasControls())
                {
                    return this.owner.Controls.Count;
                }
                return 0;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public TableRow this[int index]
        {
            get
            {
                return (TableRow) this.owner.Controls[index];
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this.owner.Controls[index];
            }
            set
            {
                this.RemoveAt(index);
                this.AddAt(index, (TableRow) value);
            }
        }
    }
}

