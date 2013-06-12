namespace System.Dynamic
{
    using System;
    using System.Collections.Generic;

    internal class ExpandoClass
    {
        private readonly int _hashCode;
        private readonly string[] _keys;
        private Dictionary<int, List<WeakReference>> _transitions;
        internal static ExpandoClass Empty = new ExpandoClass();
        private const int EmptyHashCode = 0x1997;

        internal ExpandoClass()
        {
            this._hashCode = 0x1997;
            this._keys = new string[0];
        }

        internal ExpandoClass(string[] keys, int hashCode)
        {
            this._hashCode = hashCode;
            this._keys = keys;
        }

        internal ExpandoClass FindNewClass(string newKey)
        {
            int hashCode = this._hashCode ^ newKey.GetHashCode();
            lock (this)
            {
                List<WeakReference> transitionList = this.GetTransitionList(hashCode);
                for (int i = 0; i < transitionList.Count; i++)
                {
                    ExpandoClass class2 = transitionList[i].Target as ExpandoClass;
                    if (class2 == null)
                    {
                        transitionList.RemoveAt(i);
                        i--;
                    }
                    else if (string.Equals(class2._keys[class2._keys.Length - 1], newKey, StringComparison.Ordinal))
                    {
                        return class2;
                    }
                }
                string[] destinationArray = new string[this._keys.Length + 1];
                Array.Copy(this._keys, destinationArray, this._keys.Length);
                destinationArray[this._keys.Length] = newKey;
                ExpandoClass target = new ExpandoClass(destinationArray, hashCode);
                transitionList.Add(new WeakReference(target));
                return target;
            }
        }

        private List<WeakReference> GetTransitionList(int hashCode)
        {
            List<WeakReference> list;
            if (this._transitions == null)
            {
                this._transitions = new Dictionary<int, List<WeakReference>>();
            }
            if (!this._transitions.TryGetValue(hashCode, out list))
            {
                this._transitions[hashCode] = list = new List<WeakReference>();
            }
            return list;
        }

        internal int GetValueIndex(string name, bool caseInsensitive, ExpandoObject obj)
        {
            if (caseInsensitive)
            {
                return this.GetValueIndexCaseInsensitive(name, obj);
            }
            return this.GetValueIndexCaseSensitive(name);
        }

        private int GetValueIndexCaseInsensitive(string name, ExpandoObject obj)
        {
            int num = -1;
            lock (obj.LockObject)
            {
                for (int i = this._keys.Length - 1; i >= 0; i--)
                {
                    if (string.Equals(this._keys[i], name, StringComparison.OrdinalIgnoreCase) && !obj.IsDeletedMember(i))
                    {
                        if (num != -1)
                        {
                            return -2;
                        }
                        num = i;
                    }
                }
            }
            return num;
        }

        internal int GetValueIndexCaseSensitive(string name)
        {
            for (int i = 0; i < this._keys.Length; i++)
            {
                if (string.Equals(this._keys[i], name, StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }

        internal string[] Keys
        {
            get
            {
                return this._keys;
            }
        }
    }
}

