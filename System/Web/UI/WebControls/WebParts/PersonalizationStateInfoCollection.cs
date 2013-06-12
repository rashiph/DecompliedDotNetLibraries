namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;

    [Serializable]
    public sealed class PersonalizationStateInfoCollection : ICollection, IEnumerable
    {
        private Dictionary<Key, int> _indices = new Dictionary<Key, int>(KeyComparer.Default);
        private bool _readOnly;
        private ArrayList _values = new ArrayList();

        public void Add(PersonalizationStateInfo data)
        {
            Key key;
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            UserPersonalizationStateInfo info = data as UserPersonalizationStateInfo;
            if (info != null)
            {
                key = new Key(info.Path, info.Username);
            }
            else
            {
                key = new Key(data.Path, null);
            }
            if (this._indices.ContainsKey(key))
            {
                if (info != null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("PersonalizationStateInfoCollection_CouldNotAddUserStateInfo", new object[] { key.Path, key.Username }));
                }
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationStateInfoCollection_CouldNotAddSharedStateInfo", new object[] { key.Path }));
            }
            int num = this._values.Add(data);
            try
            {
                this._indices.Add(key, num);
            }
            catch
            {
                this._values.RemoveAt(num);
                throw;
            }
        }

        public void Clear()
        {
            this._values.Clear();
            this._indices.Clear();
        }

        public void CopyTo(PersonalizationStateInfo[] array, int index)
        {
            this._values.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this._values.GetEnumerator();
        }

        public void Remove(string path, string username)
        {
            int num;
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            Key key = new Key(path, username);
            if (this._indices.TryGetValue(key, out num))
            {
                this._indices.Remove(key);
                try
                {
                    this._values.RemoveAt(num);
                }
                catch
                {
                    this._indices.Add(key, num);
                    throw;
                }
                ArrayList list = new ArrayList();
                foreach (KeyValuePair<Key, int> pair in this._indices)
                {
                    if (pair.Value > num)
                    {
                        list.Add(pair.Key);
                    }
                }
                foreach (Key key2 in list)
                {
                    this._indices[key2] -= 1;
                }
            }
        }

        public void SetReadOnly()
        {
            if (!this._readOnly)
            {
                this._readOnly = true;
                this._values = ArrayList.ReadOnly(this._values);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this._values.CopyTo(array, index);
        }

        public int Count
        {
            get
            {
                return this._values.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public PersonalizationStateInfo this[string path, string username]
        {
            get
            {
                int num;
                if (path == null)
                {
                    throw new ArgumentNullException("path");
                }
                Key key = new Key(path, username);
                if (!this._indices.TryGetValue(key, out num))
                {
                    return null;
                }
                return (PersonalizationStateInfo) this._values[num];
            }
        }

        public PersonalizationStateInfo this[int index]
        {
            get
            {
                return (PersonalizationStateInfo) this._values[index];
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        [Serializable]
        private sealed class Key
        {
            public string Path;
            public string Username;

            internal Key(string path, string username)
            {
                this.Path = path;
                this.Username = username;
            }
        }

        [Serializable]
        private sealed class KeyComparer : IEqualityComparer<PersonalizationStateInfoCollection.Key>
        {
            internal static readonly IEqualityComparer<PersonalizationStateInfoCollection.Key> Default = new PersonalizationStateInfoCollection.KeyComparer();

            private int Compare(PersonalizationStateInfoCollection.Key x, PersonalizationStateInfoCollection.Key y)
            {
                if ((x == null) && (y == null))
                {
                    return 0;
                }
                if (x == null)
                {
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                int num = string.Compare(x.Path, y.Path, StringComparison.OrdinalIgnoreCase);
                if (num != 0)
                {
                    return num;
                }
                return string.Compare(x.Username, y.Username, StringComparison.OrdinalIgnoreCase);
            }

            bool IEqualityComparer<PersonalizationStateInfoCollection.Key>.Equals(PersonalizationStateInfoCollection.Key x, PersonalizationStateInfoCollection.Key y)
            {
                return (this.Compare(x, y) == 0);
            }

            int IEqualityComparer<PersonalizationStateInfoCollection.Key>.GetHashCode(PersonalizationStateInfoCollection.Key key)
            {
                if (key == null)
                {
                    return 0;
                }
                int hashCode = key.Path.ToLowerInvariant().GetHashCode();
                int num2 = 0;
                if (key.Username != null)
                {
                    num2 = key.Username.ToLowerInvariant().GetHashCode();
                }
                return HashCodeCombiner.CombineHashCodes(hashCode, num2);
            }
        }
    }
}

