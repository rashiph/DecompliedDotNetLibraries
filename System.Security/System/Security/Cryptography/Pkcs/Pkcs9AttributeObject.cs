namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class Pkcs9AttributeObject : AsnEncodedData
    {
        public Pkcs9AttributeObject()
        {
        }

        public Pkcs9AttributeObject(AsnEncodedData asnEncodedData) : base(asnEncodedData)
        {
            if (asnEncodedData.Oid == null)
            {
                throw new ArgumentNullException("asnEncodedData.Oid");
            }
            string str = base.Oid.Value;
            if (str == null)
            {
                throw new ArgumentNullException("oid.Value");
            }
            if (str.Length == 0)
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Arg_EmptyOrNullString"), "oid.Value");
            }
        }

        internal Pkcs9AttributeObject(string oid)
        {
            base.Oid = new System.Security.Cryptography.Oid(oid);
        }

        public Pkcs9AttributeObject(System.Security.Cryptography.Oid oid, byte[] encodedData) : this(new AsnEncodedData(oid, encodedData))
        {
        }

        public Pkcs9AttributeObject(string oid, byte[] encodedData) : this(new AsnEncodedData(oid, encodedData))
        {
        }

        public override void CopyFrom(AsnEncodedData asnEncodedData)
        {
            if (asnEncodedData == null)
            {
                throw new ArgumentNullException("asnEncodedData");
            }
            if (!(asnEncodedData is Pkcs9AttributeObject))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Pkcs9_AttributeMismatch"));
            }
            base.CopyFrom(asnEncodedData);
        }

        public System.Security.Cryptography.Oid Oid
        {
            get
            {
                return base.Oid;
            }
        }
    }
}

