namespace System.Security.Cryptography
{
    using System;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CryptographicAttributeObject
    {
        private System.Security.Cryptography.Oid m_oid;
        private AsnEncodedDataCollection m_values;

        private CryptographicAttributeObject()
        {
        }

        [SecurityCritical]
        internal CryptographicAttributeObject(IntPtr pAttribute) : this((System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE) Marshal.PtrToStructure(pAttribute, typeof(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE)))
        {
        }

        internal CryptographicAttributeObject(AsnEncodedData asnEncodedData) : this(asnEncodedData.Oid, new AsnEncodedDataCollection(asnEncodedData))
        {
        }

        [SecurityCritical]
        internal CryptographicAttributeObject(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE cryptAttribute) : this(new System.Security.Cryptography.Oid(cryptAttribute.pszObjId), PkcsUtils.GetAsnEncodedDataCollection(cryptAttribute))
        {
        }

        [SecurityCritical]
        internal CryptographicAttributeObject(System.Security.Cryptography.CAPI.CRYPT_ATTRIBUTE_TYPE_VALUE cryptAttribute) : this(new System.Security.Cryptography.Oid(cryptAttribute.pszObjId), PkcsUtils.GetAsnEncodedDataCollection(cryptAttribute))
        {
        }

        public CryptographicAttributeObject(System.Security.Cryptography.Oid oid) : this(oid, new AsnEncodedDataCollection())
        {
        }

        public CryptographicAttributeObject(System.Security.Cryptography.Oid oid, AsnEncodedDataCollection values)
        {
            this.m_oid = new System.Security.Cryptography.Oid(oid);
            if (values == null)
            {
                this.m_values = new AsnEncodedDataCollection();
            }
            else
            {
                AsnEncodedDataEnumerator enumerator = values.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (string.Compare(enumerator.Current.Oid.Value, oid.Value, StringComparison.Ordinal) != 0)
                    {
                        throw new InvalidOperationException(SecurityResources.GetResourceString("InvalidOperation_DuplicateItemNotAllowed"));
                    }
                }
                this.m_values = values;
            }
        }

        public System.Security.Cryptography.Oid Oid
        {
            get
            {
                return new System.Security.Cryptography.Oid(this.m_oid);
            }
        }

        public AsnEncodedDataCollection Values
        {
            get
            {
                return this.m_values;
            }
        }
    }
}

