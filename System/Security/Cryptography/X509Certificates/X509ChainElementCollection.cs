namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public sealed class X509ChainElementCollection : ICollection, IEnumerable
    {
        private X509ChainElement[] m_elements;

        internal X509ChainElementCollection()
        {
            this.m_elements = new X509ChainElement[0];
        }

        internal unsafe X509ChainElementCollection(IntPtr pSimpleChain)
        {
            CAPIBase.CERT_SIMPLE_CHAIN structure = new CAPIBase.CERT_SIMPLE_CHAIN(Marshal.SizeOf(typeof(CAPIBase.CERT_SIMPLE_CHAIN)));
            uint size = (uint) Marshal.ReadInt32(pSimpleChain);
            if (size > Marshal.SizeOf(structure))
            {
                size = (uint) Marshal.SizeOf(structure);
            }
            System.Security.Cryptography.X509Certificates.X509Utils.memcpy(pSimpleChain, new IntPtr((void*) &structure), size);
            this.m_elements = new X509ChainElement[structure.cElement];
            for (int i = 0; i < this.m_elements.Length; i++)
            {
                this.m_elements[i] = new X509ChainElement(Marshal.ReadIntPtr(new IntPtr(((long) structure.rgpElement) + (i * Marshal.SizeOf(typeof(IntPtr))))));
            }
        }

        public void CopyTo(X509ChainElement[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public X509ChainElementEnumerator GetEnumerator()
        {
            return new X509ChainElementEnumerator(this);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(SR.GetString("Arg_RankMultiDimNotSupported"));
            }
            if ((index < 0) || (index >= array.Length))
            {
                throw new ArgumentOutOfRangeException("index", SR.GetString("ArgumentOutOfRange_Index"));
            }
            if ((index + this.Count) > array.Length)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidOffLen"));
            }
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], index);
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new X509ChainElementEnumerator(this);
        }

        public int Count
        {
            get
            {
                return this.m_elements.Length;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public X509ChainElement this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumNotStarted"));
                }
                if (index >= this.m_elements.Length)
                {
                    throw new ArgumentOutOfRangeException("index", SR.GetString("ArgumentOutOfRange_Index"));
                }
                return this.m_elements[index];
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

