namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public sealed class X509ExtensionCollection : ICollection, IEnumerable
    {
        private ArrayList m_list;

        public X509ExtensionCollection()
        {
            this.m_list = new ArrayList();
        }

        internal unsafe X509ExtensionCollection(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle)
        {
            this.m_list = new ArrayList();
            using (System.Security.Cryptography.SafeCertContextHandle handle = CAPI.CertDuplicateCertificateContext(safeCertContextHandle))
            {
                CAPIBase.CERT_CONTEXT cert_context = *((CAPIBase.CERT_CONTEXT*) handle.DangerousGetHandle());
                CAPIBase.CERT_INFO cert_info = (CAPIBase.CERT_INFO) Marshal.PtrToStructure(cert_context.pCertInfo, typeof(CAPIBase.CERT_INFO));
                uint cExtension = cert_info.cExtension;
                IntPtr rgExtension = cert_info.rgExtension;
                for (uint i = 0; i < cExtension; i++)
                {
                    X509Extension asnEncodedData = new X509Extension(new IntPtr(((long) rgExtension) + (i * Marshal.SizeOf(typeof(CAPIBase.CERT_EXTENSION)))));
                    X509Extension extension2 = CryptoConfig.CreateFromName(asnEncodedData.Oid.Value) as X509Extension;
                    if (extension2 != null)
                    {
                        extension2.CopyFrom(asnEncodedData);
                        asnEncodedData = extension2;
                    }
                    this.Add(asnEncodedData);
                }
            }
        }

        public int Add(X509Extension extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException("extension");
            }
            return this.m_list.Add(extension);
        }

        public void CopyTo(X509Extension[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public X509ExtensionEnumerator GetEnumerator()
        {
            return new X509ExtensionEnumerator(this);
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
            return new X509ExtensionEnumerator(this);
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

        public X509Extension this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumNotStarted"));
                }
                if (index >= this.m_list.Count)
                {
                    throw new ArgumentOutOfRangeException("index", SR.GetString("ArgumentOutOfRange_Index"));
                }
                return (X509Extension) this.m_list[index];
            }
        }

        public X509Extension this[string oid]
        {
            get
            {
                string strB = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(2, oid, System.Security.Cryptography.OidGroup.ExtensionOrAttribute);
                if (strB == null)
                {
                    strB = oid;
                }
                foreach (X509Extension extension in this.m_list)
                {
                    if (string.Compare(extension.Oid.Value, strB, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return extension;
                    }
                }
                return null;
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

