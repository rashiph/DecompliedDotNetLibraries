namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class SignatureDescription
    {
        private string _strDeformatter;
        private string _strDigest;
        private string _strFormatter;
        private string _strKey;

        public SignatureDescription()
        {
        }

        public SignatureDescription(SecurityElement el)
        {
            if (el == null)
            {
                throw new ArgumentNullException("el");
            }
            this._strKey = el.SearchForTextOfTag("Key");
            this._strDigest = el.SearchForTextOfTag("Digest");
            this._strFormatter = el.SearchForTextOfTag("Formatter");
            this._strDeformatter = el.SearchForTextOfTag("Deformatter");
        }

        [SecuritySafeCritical]
        public virtual AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
        {
            AsymmetricSignatureDeformatter deformatter = (AsymmetricSignatureDeformatter) CryptoConfig.CreateFromName(this._strDeformatter);
            deformatter.SetKey(key);
            return deformatter;
        }

        [SecuritySafeCritical]
        public virtual HashAlgorithm CreateDigest()
        {
            return (HashAlgorithm) CryptoConfig.CreateFromName(this._strDigest);
        }

        [SecuritySafeCritical]
        public virtual AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
        {
            AsymmetricSignatureFormatter formatter = (AsymmetricSignatureFormatter) CryptoConfig.CreateFromName(this._strFormatter);
            formatter.SetKey(key);
            return formatter;
        }

        public string DeformatterAlgorithm
        {
            get
            {
                return this._strDeformatter;
            }
            set
            {
                this._strDeformatter = value;
            }
        }

        public string DigestAlgorithm
        {
            get
            {
                return this._strDigest;
            }
            set
            {
                this._strDigest = value;
            }
        }

        public string FormatterAlgorithm
        {
            get
            {
                return this._strFormatter;
            }
            set
            {
                this._strFormatter = value;
            }
        }

        public string KeyAlgorithm
        {
            get
            {
                return this._strKey;
            }
            set
            {
                this._strKey = value;
            }
        }
    }
}

