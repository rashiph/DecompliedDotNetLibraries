namespace System.Web.Security
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [Serializable, TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class MembershipUserCollection : ICollection, IEnumerable
    {
        private Hashtable _Indices = new Hashtable(10, StringComparer.CurrentCultureIgnoreCase);
        private bool _ReadOnly;
        private ArrayList _Values = new ArrayList();

        public void Add(MembershipUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (this._ReadOnly)
            {
                throw new NotSupportedException();
            }
            int num = this._Values.Add(user);
            try
            {
                this._Indices.Add(user.UserName, num);
            }
            catch
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

        public void CopyTo(MembershipUser[] array, int index)
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

        void ICollection.CopyTo(Array array, int index)
        {
            this._Values.CopyTo(array, index);
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

        public MembershipUser this[string name]
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
                return (MembershipUser) this._Values[num];
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

