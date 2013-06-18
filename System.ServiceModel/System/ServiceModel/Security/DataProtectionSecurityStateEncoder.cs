namespace System.ServiceModel.Security
{
    using System;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.Text;

    public class DataProtectionSecurityStateEncoder : SecurityStateEncoder
    {
        private byte[] entropy;
        private bool useCurrentUserProtectionScope;

        public DataProtectionSecurityStateEncoder() : this(true)
        {
        }

        public DataProtectionSecurityStateEncoder(bool useCurrentUserProtectionScope) : this(useCurrentUserProtectionScope, null)
        {
        }

        public DataProtectionSecurityStateEncoder(bool useCurrentUserProtectionScope, byte[] entropy)
        {
            this.useCurrentUserProtectionScope = useCurrentUserProtectionScope;
            if (entropy == null)
            {
                this.entropy = null;
            }
            else
            {
                this.entropy = DiagnosticUtility.Utility.AllocateByteArray(entropy.Length);
                Buffer.BlockCopy(entropy, 0, this.entropy, 0, entropy.Length);
            }
        }

        protected internal override byte[] DecodeSecurityState(byte[] data)
        {
            byte[] buffer;
            try
            {
                buffer = ProtectedData.Unprotect(data, this.entropy, this.useCurrentUserProtectionScope ? DataProtectionScope.CurrentUser : DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.ServiceModel.SR.GetString("SecurityStateEncoderDecodingFailure"), exception));
            }
            return buffer;
        }

        protected internal override byte[] EncodeSecurityState(byte[] data)
        {
            byte[] buffer;
            try
            {
                buffer = ProtectedData.Protect(data, this.entropy, this.useCurrentUserProtectionScope ? DataProtectionScope.CurrentUser : DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.ServiceModel.SR.GetString("SecurityStateEncoderEncodingFailure"), exception));
            }
            return buffer;
        }

        public byte[] GetEntropy()
        {
            byte[] dst = null;
            if (this.entropy != null)
            {
                dst = DiagnosticUtility.Utility.AllocateByteArray(this.entropy.Length);
                Buffer.BlockCopy(this.entropy, 0, dst, 0, this.entropy.Length);
            }
            return dst;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(base.GetType().ToString());
            builder.AppendFormat("{0}  UseCurrentUserProtectionScope={1}", Environment.NewLine, this.useCurrentUserProtectionScope);
            builder.AppendFormat("{0}  Entropy Length={1}", Environment.NewLine, (this.entropy == null) ? 0 : this.entropy.Length);
            return builder.ToString();
        }

        public bool UseCurrentUserProtectionScope
        {
            get
            {
                return this.useCurrentUserProtectionScope;
            }
        }
    }
}

