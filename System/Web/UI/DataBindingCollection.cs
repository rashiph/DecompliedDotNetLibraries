namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class DataBindingCollection : ICollection, IEnumerable
    {
        private Hashtable bindings = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private Hashtable removedBindings;

        public event EventHandler Changed;

        public void Add(DataBinding binding)
        {
            this.bindings[binding.PropertyName] = binding;
            this.RemovedBindingsTable.Remove(binding.PropertyName);
            this.OnChanged();
        }

        public void Clear()
        {
            ICollection keys = this.bindings.Keys;
            if ((keys.Count != 0) && (this.removedBindings == null))
            {
                Hashtable removedBindingsTable = this.RemovedBindingsTable;
            }
            foreach (string str in keys)
            {
                this.removedBindings[str] = string.Empty;
            }
            this.bindings.Clear();
            this.OnChanged();
        }

        public bool Contains(string propertyName)
        {
            return this.bindings.Contains(propertyName);
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
            return this.bindings.Values.GetEnumerator();
        }

        private void OnChanged()
        {
            if (this.changedEvent != null)
            {
                this.changedEvent(this, EventArgs.Empty);
            }
        }

        public void Remove(string propertyName)
        {
            this.Remove(propertyName, true);
        }

        public void Remove(DataBinding binding)
        {
            this.Remove(binding.PropertyName, true);
        }

        public void Remove(string propertyName, bool addToRemovedList)
        {
            if (this.Contains(propertyName))
            {
                this.bindings.Remove(propertyName);
                if (addToRemovedList)
                {
                    this.RemovedBindingsTable[propertyName] = string.Empty;
                }
                this.OnChanged();
            }
        }

        public int Count
        {
            get
            {
                return this.bindings.Count;
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

        public DataBinding this[string propertyName]
        {
            get
            {
                object obj2 = this.bindings[propertyName];
                if (obj2 != null)
                {
                    return (DataBinding) obj2;
                }
                return null;
            }
        }

        public string[] RemovedBindings
        {
            get
            {
                ICollection keys = null;
                if (this.removedBindings == null)
                {
                    return new string[0];
                }
                keys = this.removedBindings.Keys;
                string[] strArray = new string[keys.Count];
                int num2 = 0;
                foreach (string str in keys)
                {
                    strArray[num2++] = str;
                }
                this.removedBindings.Clear();
                return strArray;
            }
        }

        private Hashtable RemovedBindingsTable
        {
            get
            {
                if (this.removedBindings == null)
                {
                    this.removedBindings = new Hashtable(StringComparer.OrdinalIgnoreCase);
                }
                return this.removedBindings;
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

