namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class SettingsPropertyCollection : ICloneable, ICollection, IEnumerable
    {
        private Hashtable _Hashtable;
        private bool _ReadOnly;

        public SettingsPropertyCollection()
        {
            this._Hashtable = new Hashtable(10, StringComparer.CurrentCultureIgnoreCase);
        }

        private SettingsPropertyCollection(Hashtable h)
        {
            this._Hashtable = (Hashtable) h.Clone();
        }

        public void Add(SettingsProperty property)
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException();
            }
            this.OnAdd(property);
            this._Hashtable.Add(property.Name, property);
            try
            {
                this.OnAddComplete(property);
            }
            catch
            {
                this._Hashtable.Remove(property.Name);
                throw;
            }
        }

        public void Clear()
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException();
            }
            this.OnClear();
            this._Hashtable.Clear();
            this.OnClearComplete();
        }

        public object Clone()
        {
            return new SettingsPropertyCollection(this._Hashtable);
        }

        public void CopyTo(Array array, int index)
        {
            this._Hashtable.Values.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this._Hashtable.Values.GetEnumerator();
        }

        protected virtual void OnAdd(SettingsProperty property)
        {
        }

        protected virtual void OnAddComplete(SettingsProperty property)
        {
        }

        protected virtual void OnClear()
        {
        }

        protected virtual void OnClearComplete()
        {
        }

        protected virtual void OnRemove(SettingsProperty property)
        {
        }

        protected virtual void OnRemoveComplete(SettingsProperty property)
        {
        }

        public void Remove(string name)
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException();
            }
            SettingsProperty property = (SettingsProperty) this._Hashtable[name];
            if (property != null)
            {
                this.OnRemove(property);
                this._Hashtable.Remove(name);
                try
                {
                    this.OnRemoveComplete(property);
                }
                catch
                {
                    this._Hashtable.Add(name, property);
                    throw;
                }
            }
        }

        public void SetReadOnly()
        {
            if (!this._ReadOnly)
            {
                this._ReadOnly = true;
            }
        }

        public int Count
        {
            get
            {
                return this._Hashtable.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public SettingsProperty this[string name]
        {
            get
            {
                return (this._Hashtable[name] as SettingsProperty);
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

