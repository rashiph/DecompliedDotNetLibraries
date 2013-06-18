namespace System.Deployment.Application
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Globalization;

    internal class Hash
    {
        private System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD _digestMethod;
        private byte[] _digestValue;
        private System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM _transform;

        public Hash(byte[] digestValue, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD digestMethod, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM transform)
        {
            if (digestValue == null)
            {
                throw new ArgumentException(Resources.GetString("Ex_HashNullDigestValue"));
            }
            this._digestValue = digestValue;
            this._digestMethod = digestMethod;
            this._transform = transform;
        }

        protected static string ToCodedString(uint value)
        {
            if (value > 0xff)
            {
                throw new ArgumentException(Resources.GetString("Ex_CodeLimitExceeded"));
            }
            return string.Format(CultureInfo.InvariantCulture, "{0:x2}", new object[] { value });
        }

        public string CompositString
        {
            get
            {
                return (this.DigestMethodCodeString + this.TranformCodeString + HexString.FromBytes(this.DigestValue));
            }
        }

        public System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD DigestMethod
        {
            get
            {
                return this._digestMethod;
            }
        }

        protected string DigestMethodCodeString
        {
            get
            {
                return ToCodedString((uint) this.DigestMethod);
            }
        }

        public byte[] DigestValue
        {
            get
            {
                return this._digestValue;
            }
        }

        protected string TranformCodeString
        {
            get
            {
                return ToCodedString((uint) this.Transform);
            }
        }

        public System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM Transform
        {
            get
            {
                return this._transform;
            }
        }
    }
}

