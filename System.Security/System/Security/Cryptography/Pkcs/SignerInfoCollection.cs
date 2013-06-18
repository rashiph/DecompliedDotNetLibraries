namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class SignerInfoCollection : ICollection, IEnumerable
    {
        private SignerInfo[] m_signerInfos;

        internal SignerInfoCollection()
        {
            this.m_signerInfos = new SignerInfo[0];
        }

        [SecuritySafeCritical]
        internal unsafe SignerInfoCollection(SignedCms signedCms)
        {
            uint num = 0;
            uint num2 = (uint) Marshal.SizeOf(typeof(uint));
            System.Security.Cryptography.SafeCryptMsgHandle cryptMsgHandle = signedCms.GetCryptMsgHandle();
            if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(cryptMsgHandle, 5, 0, new IntPtr((void*) &num), new IntPtr((void*) &num2)))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            SignerInfo[] infoArray = new SignerInfo[num];
            for (int i = 0; i < num; i++)
            {
                uint num4 = 0;
                if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(cryptMsgHandle, 6, (uint) i, IntPtr.Zero, new IntPtr((void*) &num4)))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                System.Security.Cryptography.SafeLocalAllocHandle pvData = System.Security.Cryptography.CAPI.LocalAlloc(0, new IntPtr((long) num4));
                if (!System.Security.Cryptography.CAPI.CAPISafe.CryptMsgGetParam(cryptMsgHandle, 6, (uint) i, pvData, new IntPtr((void*) &num4)))
                {
                    throw new CryptographicException(Marshal.GetLastWin32Error());
                }
                infoArray[i] = new SignerInfo(signedCms, pvData);
            }
            this.m_signerInfos = infoArray;
        }

        [SecuritySafeCritical]
        internal SignerInfoCollection(SignedCms signedCms, SignerInfo signerInfo)
        {
            SignerInfo[] infoArray = new SignerInfo[0];
            int num = 0;
            int num2 = 0;
            CryptographicAttributeObjectEnumerator enumerator = signerInfo.UnsignedAttributes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CryptographicAttributeObject current = enumerator.Current;
                if (current.Oid.Value == "1.2.840.113549.1.9.6")
                {
                    num += current.Values.Count;
                }
            }
            infoArray = new SignerInfo[num];
            CryptographicAttributeObjectEnumerator enumerator2 = signerInfo.UnsignedAttributes.GetEnumerator();
            while (enumerator2.MoveNext())
            {
                CryptographicAttributeObject obj3 = enumerator2.Current;
                if (obj3.Oid.Value == "1.2.840.113549.1.9.6")
                {
                    for (int i = 0; i < obj3.Values.Count; i++)
                    {
                        AsnEncodedData data = obj3.Values[i];
                        infoArray[num2++] = new SignerInfo(signedCms, signerInfo, data.RawData);
                    }
                }
            }
            this.m_signerInfos = infoArray;
        }

        public void CopyTo(Array array, int index)
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

        public void CopyTo(SignerInfo[] array, int index)
        {
            this.CopyTo(array, index);
        }

        public SignerInfoEnumerator GetEnumerator()
        {
            return new SignerInfoEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SignerInfoEnumerator(this);
        }

        public int Count
        {
            get
            {
                return this.m_signerInfos.Length;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public SignerInfo this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.m_signerInfos.Length))
                {
                    throw new ArgumentOutOfRangeException("index", SecurityResources.GetResourceString("ArgumentOutOfRange_Index"));
                }
                return this.m_signerInfos[index];
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

