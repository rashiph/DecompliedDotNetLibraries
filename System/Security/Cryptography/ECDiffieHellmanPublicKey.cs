namespace System.Security.Cryptography
{
    using System;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class ECDiffieHellmanPublicKey : IDisposable
    {
        private byte[] m_keyBlob;

        protected ECDiffieHellmanPublicKey(byte[] keyBlob)
        {
            if (keyBlob == null)
            {
                throw new ArgumentNullException("keyBlob");
            }
            this.m_keyBlob = keyBlob.Clone() as byte[];
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public virtual byte[] ToByteArray()
        {
            return (this.m_keyBlob.Clone() as byte[]);
        }

        public abstract string ToXmlString();
    }
}

