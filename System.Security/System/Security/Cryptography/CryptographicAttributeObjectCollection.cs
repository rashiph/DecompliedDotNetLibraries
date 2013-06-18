namespace System.Security.Cryptography
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CryptographicAttributeObjectCollection : ICollection, IEnumerable
    {
        private ArrayList m_list;

        public CryptographicAttributeObjectCollection()
        {
            this.m_list = new ArrayList();
        }

        [SecurityCritical]
        private CryptographicAttributeObjectCollection(IntPtr pCryptAttributes) : this((System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTES) Marshal.PtrToStructure(pCryptAttributes, typeof(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTES)))
        {
        }

        [SecurityCritical]
        internal CryptographicAttributeObjectCollection(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTES cryptAttributes)
        {
            this.m_list = new ArrayList();
            for (uint i = 0; i < cryptAttributes.cAttr; i++)
            {
                IntPtr pAttribute = new IntPtr(((long) cryptAttributes.rgAttr) + (i * Marshal.SizeOf(typeof(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE))));
                this.m_list.Add(new CryptographicAttributeObject(pAttribute));
            }
        }

        public CryptographicAttributeObjectCollection(CryptographicAttributeObject attribute)
        {
            this.m_list = new ArrayList();
            this.m_list.Add(attribute);
        }

        [SecurityCritical]
        internal CryptographicAttributeObjectCollection(System.Security.Cryptography.SafeLocalAllocHandle pCryptAttributes) : this(pCryptAttributes.DangerousGetHandle())
        {
        }

        public int Add(AsnEncodedData asnEncodedData)
        {
            if (asnEncodedData == null)
            {
                throw new ArgumentNullException("asnEncodedData");
            }
            return this.Add(new CryptographicAttributeObject(asnEncodedData));
        }

        public int Add(CryptographicAttributeObject attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            string strA = null;
            if (attribute.Oid != null)
            {
                strA = attribute.Oid.Value;
            }
            for (int i = 0; i < this.m_list.Count; i++)
            {
                CryptographicAttributeObject obj2 = (CryptographicAttributeObject) this.m_list[i];
                if (obj2.Values == attribute.Values)
                {
                    throw new InvalidOperationException(SecurityResources.GetResourceString("InvalidOperation_DuplicateItemNotAllowed"));
                }
                string strB = null;
                if (obj2.Oid != null)
                {
                    strB = obj2.Oid.Value;
                }
                if ((strA == null) && (strB == null))
                {
                    AsnEncodedDataEnumerator enumerator = attribute.Values.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        AsnEncodedData current = enumerator.Current;
                        obj2.Values.Add(current);
                    }
                    return i;
                }
                if (((strA != null) && (strB != null)) && (string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    if (string.Compare(strA, "1.2.840.113549.1.9.5", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Pkcs9_MultipleSigningTimeNotAllowed"));
                    }
                    AsnEncodedDataEnumerator enumerator2 = attribute.Values.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        AsnEncodedData asnEncodedData = enumerator2.Current;
                        obj2.Values.Add(asnEncodedData);
                    }
                    return i;
                }
            }
            return this.m_list.Add(attribute);
        }

        public void CopyTo(CryptographicAttributeObject[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public CryptographicAttributeObjectEnumerator GetEnumerator()
        {
            return new CryptographicAttributeObjectEnumerator(this);
        }

        public void Remove(CryptographicAttributeObject attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            this.m_list.Remove(attribute);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Arg_RankMultiDimNotSupported"));
            }
            if ((index < 0) || (index >= array.Length))
            {
                throw new ArgumentOutOfRangeException("index", SecurityResources.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((index + this.Count) > array.Length)
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Argument_InvalidOffLen"));
            }
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], index);
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CryptographicAttributeObjectEnumerator(this);
        }

        public int Count
        {
            get
            {
                return this.m_list.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public CryptographicAttributeObject this[int index]
        {
            get
            {
                return (CryptographicAttributeObject) this.m_list[index];
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

