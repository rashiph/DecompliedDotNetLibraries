namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class ReferenceList : IList, ICollection, IEnumerable
    {
        private ArrayList m_references = new ArrayList();

        public int Add(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(value is DataReference) && !(value is KeyReference))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "value");
            }
            return this.m_references.Add(value);
        }

        public void Clear()
        {
            this.m_references.Clear();
        }

        public bool Contains(object value)
        {
            return this.m_references.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            this.m_references.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.m_references.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return this.m_references.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(value is DataReference) && !(value is KeyReference))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "value");
            }
            this.m_references.Insert(index, value);
        }

        public EncryptedReference Item(int index)
        {
            return (EncryptedReference) this.m_references[index];
        }

        public void Remove(object value)
        {
            this.m_references.Remove(value);
        }

        public void RemoveAt(int index)
        {
            this.m_references.RemoveAt(index);
        }

        public int Count
        {
            get
            {
                return this.m_references.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this.m_references.IsSynchronized;
            }
        }

        public EncryptedReference this[int index]
        {
            get
            {
                return this.Item(index);
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
                return this.m_references.SyncRoot;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return this.m_references.IsFixedSize;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this.m_references.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this.m_references[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!(value is DataReference) && !(value is KeyReference))
                {
                    throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_IncorrectObjectType"), "value");
                }
                this.m_references[index] = value;
            }
        }
    }
}

