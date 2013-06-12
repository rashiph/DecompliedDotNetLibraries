namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true), SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public abstract class BaseChannelObjectWithProperties : IDictionary, ICollection, IEnumerable
    {
        protected BaseChannelObjectWithProperties()
        {
        }

        [SecuritySafeCritical]
        public virtual void Add(object key, object value)
        {
            throw new NotSupportedException();
        }

        [SecuritySafeCritical]
        public virtual void Clear()
        {
            throw new NotSupportedException();
        }

        [SecuritySafeCritical]
        public virtual bool Contains(object key)
        {
            if (key != null)
            {
                ICollection keys = this.Keys;
                if (keys == null)
                {
                    return false;
                }
                string strA = key as string;
                foreach (object obj2 in keys)
                {
                    if (strA != null)
                    {
                        string strB = obj2 as string;
                        if (strB != null)
                        {
                            if (string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                continue;
                            }
                            return true;
                        }
                    }
                    if (key.Equals(obj2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [SecuritySafeCritical]
        public virtual void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        [SecuritySafeCritical]
        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return new DictionaryEnumeratorByKeys(this);
        }

        [SecuritySafeCritical]
        public virtual void Remove(object key)
        {
            throw new NotSupportedException();
        }

        [SecuritySafeCritical]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DictionaryEnumeratorByKeys(this);
        }

        public virtual int Count
        {
            [SecuritySafeCritical]
            get
            {
                ICollection keys = this.Keys;
                if (keys == null)
                {
                    return 0;
                }
                return keys.Count;
            }
        }

        public virtual bool IsFixedSize
        {
            [SecuritySafeCritical]
            get
            {
                return true;
            }
        }

        public virtual bool IsReadOnly
        {
            [SecuritySafeCritical]
            get
            {
                return false;
            }
        }

        public virtual bool IsSynchronized
        {
            [SecuritySafeCritical]
            get
            {
                return false;
            }
        }

        public virtual object this[object key]
        {
            [SecuritySafeCritical]
            get
            {
                return null;
            }
            [SecuritySafeCritical]
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual ICollection Keys
        {
            [SecuritySafeCritical]
            get
            {
                return null;
            }
        }

        public virtual IDictionary Properties
        {
            [SecurityCritical]
            get
            {
                return this;
            }
        }

        public virtual object SyncRoot
        {
            [SecuritySafeCritical]
            get
            {
                return this;
            }
        }

        public virtual ICollection Values
        {
            [SecuritySafeCritical]
            get
            {
                ICollection keys = this.Keys;
                if (keys == null)
                {
                    return null;
                }
                ArrayList list = new ArrayList();
                foreach (object obj2 in keys)
                {
                    list.Add(this[obj2]);
                }
                return list;
            }
        }
    }
}

