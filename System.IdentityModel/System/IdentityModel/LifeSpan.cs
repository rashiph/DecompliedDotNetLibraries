namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    internal class LifeSpan
    {
        private DateTime effectiveTimeUtc;
        private DateTime expiryTimeUtc;

        internal unsafe LifeSpan(byte[] buffer)
        {
            fixed (byte* numRef = buffer)
            {
                IntPtr ptr = new IntPtr((void*) numRef);
                LifeSpan_Struct struct2 = (LifeSpan_Struct) Marshal.PtrToStructure(ptr, typeof(LifeSpan_Struct));
                this.effectiveTimeUtc = DateTime.FromFileTimeUtc(struct2.start) + (DateTime.UtcNow - DateTime.Now);
                this.expiryTimeUtc = DateTime.FromFileTimeUtc(struct2.end) + (DateTime.UtcNow - DateTime.Now);
            }
        }

        internal DateTime EffectiveTimeUtc
        {
            get
            {
                return this.effectiveTimeUtc;
            }
        }

        internal DateTime ExpiryTimeUtc
        {
            get
            {
                return this.expiryTimeUtc;
            }
        }
    }
}

