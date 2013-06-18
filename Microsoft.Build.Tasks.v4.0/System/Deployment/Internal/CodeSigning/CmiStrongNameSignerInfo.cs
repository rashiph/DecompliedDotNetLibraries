namespace System.Deployment.Internal.CodeSigning
{
    using System;
    using System.Security.Cryptography;

    internal class CmiStrongNameSignerInfo
    {
        private int m_error;
        private string m_publicKeyToken;
        private AsymmetricAlgorithm m_snKey;

        internal CmiStrongNameSignerInfo()
        {
        }

        internal CmiStrongNameSignerInfo(int errorCode, string publicKeyToken)
        {
            this.m_error = errorCode;
            this.m_publicKeyToken = publicKeyToken;
        }

        internal int ErrorCode
        {
            get
            {
                return this.m_error;
            }
            set
            {
                this.m_error = value;
            }
        }

        internal AsymmetricAlgorithm PublicKey
        {
            get
            {
                return this.m_snKey;
            }
            set
            {
                this.m_snKey = value;
            }
        }

        internal string PublicKeyToken
        {
            get
            {
                return this.m_publicKeyToken;
            }
            set
            {
                this.m_publicKeyToken = value;
            }
        }
    }
}

