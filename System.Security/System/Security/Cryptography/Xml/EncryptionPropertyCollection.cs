namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EncryptionPropertyCollection : IList, ICollection, IEnumerable
    {
        private ArrayList m_props = new ArrayList();

        public int Add(EncryptionProperty value)
        {
            return this.m_props.Add(value);
        }

        public void Clear()
        {
            this.m_props.Clear();
        }

        public bool Contains(EncryptionProperty value)
        {
            return this.m_props.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            this.m_props.CopyTo(array, index);
        }

        public void CopyTo(EncryptionProperty[] array, int index)
        {
            this.m_props.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.m_props.GetEnumerator();
        }

        public int IndexOf(EncryptionProperty value)
        {
            return this.m_props.IndexOf(value);
        }

        public void Insert(int index, EncryptionProperty value)
        {
            this.m_props.Insert(index, value);
        }

        public EncryptionProperty Item(int index)
        {
            return (EncryptionProperty) this.m_props[index];
        }

        public void Remove(EncryptionProperty value)
        {
            this.m_props.Remove(value);
        }

        public void RemoveAt(int index)
        {
            this.m_props.RemoveAt(index);
        }

        int IList.Add(object value)
        {
            if (!(value is EncryptionProperty))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "value");
            }
            return this.m_props.Add(value);
        }

        bool IList.Contains(object value)
        {
            if (!(value is EncryptionProperty))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "value");
            }
            return this.m_props.Contains(value);
        }

        int IList.IndexOf(object value)
        {
            if (!(value is EncryptionProperty))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "value");
            }
            return this.m_props.IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            if (!(value is EncryptionProperty))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "value");
            }
            this.m_props.Insert(index, value);
        }

        void IList.Remove(object value)
        {
            if (!(value is EncryptionProperty))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "value");
            }
            this.m_props.Remove(value);
        }

        public int Count
        {
            get
            {
                return this.m_props.Count;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return this.m_props.IsFixedSize;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.m_props.IsReadOnly;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this.m_props.IsSynchronized;
            }
        }

        public EncryptionProperty this[int index]
        {
            get
            {
                return (EncryptionProperty) ((IList) this)[index];
            }
            set
            {
                ((IList) this)[index] = value;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.m_props.SyncRoot;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this.m_props[index];
            }
            set
            {
                if (!(value is EncryptionProperty))
                {
                    throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "value");
                }
                this.m_props[index] = value;
            }
        }
    }
}

