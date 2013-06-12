namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class SettingsPropertyValueCollection : ICloneable, ICollection, IEnumerable
    {
        private Hashtable _Indices;
        private bool _ReadOnly;
        private ArrayList _Values;

        public SettingsPropertyValueCollection()
        {
            this._Indices = new Hashtable(10, StringComparer.CurrentCultureIgnoreCase);
            this._Values = new ArrayList();
        }

        private SettingsPropertyValueCollection(Hashtable indices, ArrayList values)
        {
            this._Indices = (Hashtable) indices.Clone();
            this._Values = (ArrayList) values.Clone();
        }

        public void Add(SettingsPropertyValue property)
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException();
            }
            int num = this._Values.Add(property);
            try
            {
                this._Indices.Add(property.Name, num);
            }
            catch (Exception)
            {
                this._Values.RemoveAt(num);
                throw;
            }
        }

        public void Clear()
        {
            this._Values.Clear();
            this._Indices.Clear();
        }

        public object Clone()
        {
            return new SettingsPropertyValueCollection(this._Indices, this._Values);
        }

        public void CopyTo(Array array, int index)
        {
            this._Values.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this._Values.GetEnumerator();
        }

        public void Remove(string name)
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException();
            }
            object obj2 = this._Indices[name];
            if ((obj2 != null) && (obj2 is int))
            {
                int index = (int) obj2;
                if (index < this._Values.Count)
                {
                    this._Values.RemoveAt(index);
                    this._Indices.Remove(name);
                    ArrayList list = new ArrayList();
                    foreach (DictionaryEntry entry in this._Indices)
                    {
                        if (((int) entry.Value) > index)
                        {
                            list.Add(entry.Key);
                        }
                    }
                    foreach (string str in list)
                    {
                        this._Indices[str] = ((int) this._Indices[str]) - 1;
                    }
                }
            }
        }

        public void SetReadOnly()
        {
            if (!this._ReadOnly)
            {
                this._ReadOnly = true;
                this._Values = ArrayList.ReadOnly(this._Values);
            }
        }

        public int Count
        {
            get
            {
                return this._Values.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public SettingsPropertyValue this[string name]
        {
            get
            {
                object obj2 = this._Indices[name];
                if ((obj2 == null) || !(obj2 is int))
                {
                    return null;
                }
                int num = (int) obj2;
                if (num >= this._Values.Count)
                {
                    return null;
                }
                return (SettingsPropertyValue) this._Values[num];
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

