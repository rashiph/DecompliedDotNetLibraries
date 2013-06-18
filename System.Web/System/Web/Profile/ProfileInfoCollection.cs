namespace System.Web.Profile
{
    using System;
    using System.Collections;
    using System.Reflection;

    [Serializable]
    public sealed class ProfileInfoCollection : ICollection, IEnumerable
    {
        private ArrayList _ArrayList = new ArrayList();
        private int _CurPos;
        private Hashtable _Hashtable = new Hashtable(10, StringComparer.CurrentCultureIgnoreCase);
        private int _NumBlanks;
        private bool _ReadOnly;

        public void Add(ProfileInfo profileInfo)
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException();
            }
            if ((profileInfo == null) || (profileInfo.UserName == null))
            {
                throw new ArgumentNullException("profileInfo");
            }
            this._Hashtable.Add(profileInfo.UserName, this._CurPos);
            this._ArrayList.Add(profileInfo);
            this._CurPos++;
        }

        public void Clear()
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException();
            }
            this._Hashtable.Clear();
            this._ArrayList.Clear();
            this._CurPos = 0;
            this._NumBlanks = 0;
        }

        public void CopyTo(Array array, int index)
        {
            this.DoCompact();
            this._ArrayList.CopyTo(array, index);
        }

        public void CopyTo(ProfileInfo[] array, int index)
        {
            this.DoCompact();
            this._ArrayList.CopyTo(array, index);
        }

        private void DoCompact()
        {
            if (this._NumBlanks >= 1)
            {
                ArrayList list = new ArrayList(this._CurPos - this._NumBlanks);
                int num = -1;
                for (int i = 0; i < this._CurPos; i++)
                {
                    if (this._ArrayList[i] != null)
                    {
                        list.Add(this._ArrayList[i]);
                    }
                    else if (num == -1)
                    {
                        num = i;
                    }
                }
                this._NumBlanks = 0;
                this._ArrayList = list;
                this._CurPos = this._ArrayList.Count;
                for (int j = num; j < this._CurPos; j++)
                {
                    ProfileInfo info = this._ArrayList[j] as ProfileInfo;
                    this._Hashtable[info.UserName] = j;
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            this.DoCompact();
            return this._ArrayList.GetEnumerator();
        }

        public void Remove(string name)
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException();
            }
            object obj2 = this._Hashtable[name];
            if (obj2 != null)
            {
                this._Hashtable.Remove(name);
                this._ArrayList[(int) obj2] = null;
                this._NumBlanks++;
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

        public ProfileInfo this[string name]
        {
            get
            {
                object obj2 = this._Hashtable[name];
                if (obj2 == null)
                {
                    return null;
                }
                return (this._ArrayList[(int) obj2] as ProfileInfo);
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

