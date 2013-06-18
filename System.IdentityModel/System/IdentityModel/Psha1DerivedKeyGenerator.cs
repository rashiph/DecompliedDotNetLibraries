namespace System.IdentityModel
{
    using System;
    using System.Security.Cryptography;

    internal sealed class Psha1DerivedKeyGenerator
    {
        private byte[] key;

        public Psha1DerivedKeyGenerator(byte[] key)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            this.key = key;
        }

        public byte[] GenerateDerivedKey(byte[] label, byte[] nonce, int derivedKeySize, int position)
        {
            if (label == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("label");
            }
            if (nonce == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("nonce");
            }
            ManagedPsha1 psha = new ManagedPsha1(this.key, label, nonce);
            return psha.GetDerivedKey(derivedKeySize, position);
        }

        private sealed class ManagedPsha1
        {
            private byte[] aValue;
            private byte[] buffer;
            private byte[] chunk;
            private KeyedHashAlgorithm hmac;
            private int index;
            private int position;
            private byte[] secret;
            private byte[] seed;

            public ManagedPsha1(byte[] secret, byte[] label, byte[] seed)
            {
                this.secret = secret;
                this.seed = DiagnosticUtility.Utility.AllocateByteArray(label.Length + seed.Length);
                label.CopyTo(this.seed, 0);
                seed.CopyTo(this.seed, label.Length);
                this.aValue = this.seed;
                this.chunk = new byte[0];
                this.index = 0;
                this.position = 0;
                this.hmac = CryptoHelper.NewHmacSha1KeyedHashAlgorithm(secret);
                this.buffer = DiagnosticUtility.Utility.AllocateByteArray((this.hmac.HashSize / 8) + this.seed.Length);
            }

            private byte GetByte()
            {
                if (this.index >= this.chunk.Length)
                {
                    this.hmac.Initialize();
                    this.aValue = this.hmac.ComputeHash(this.aValue);
                    this.aValue.CopyTo(this.buffer, 0);
                    this.seed.CopyTo(this.buffer, this.aValue.Length);
                    this.hmac.Initialize();
                    this.chunk = this.hmac.ComputeHash(this.buffer);
                    this.index = 0;
                }
                this.position++;
                return this.chunk[this.index++];
            }

            public byte[] GetDerivedKey(int derivedKeySize, int position)
            {
                if (derivedKeySize < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("derivedKeySize", System.IdentityModel.SR.GetString("ValueMustBeNonNegative")));
                }
                if (this.position > position)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("position", System.IdentityModel.SR.GetString("ValueMustBeInRange", new object[] { 0, this.position })));
                }
                while (this.position < position)
                {
                    this.GetByte();
                }
                int num = derivedKeySize / 8;
                byte[] buffer = new byte[num];
                for (int i = 0; i < num; i++)
                {
                    buffer[i] = this.GetByte();
                }
                return buffer;
            }
        }
    }
}

