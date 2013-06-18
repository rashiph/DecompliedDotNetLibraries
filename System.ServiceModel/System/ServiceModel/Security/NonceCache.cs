namespace System.ServiceModel.Security
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;

    internal sealed class NonceCache
    {
        private NonceCacheImpl cacheImpl;
        private TimeSpan cachingTime;
        private int maxCachedNonces;

        public NonceCache(TimeSpan cachingTime, int maxCachedNonces)
        {
            if (cachingTime <= TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("cachingTime", System.ServiceModel.SR.GetString("TimeSpanMustbeGreaterThanTimeSpanZero")));
            }
            this.cacheImpl = new NonceCacheImpl(cachingTime, maxCachedNonces);
            this.cachingTime = cachingTime;
            this.maxCachedNonces = maxCachedNonces;
        }

        public bool CheckNonce(byte[] nonce)
        {
            return this.cacheImpl.CheckNonce(nonce);
        }

        public override string ToString()
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            writer.WriteLine("NonceCache:");
            writer.WriteLine("   Caching Timespan: {0}", this.CachingTimeSpan);
            writer.WriteLine("   Capacity: {0}", this.MaxCachedNonces);
            return writer.ToString();
        }

        public bool TryAddNonce(byte[] nonce)
        {
            return this.cacheImpl.TryAddNonce(nonce);
        }

        public TimeSpan CachingTimeSpan
        {
            get
            {
                return this.cachingTime;
            }
        }

        public int MaxCachedNonces
        {
            get
            {
                return this.maxCachedNonces;
            }
        }

        internal sealed class NonceCacheImpl : TimeBoundedCache
        {
            private TimeSpan cachingTimeSpan;
            private static NonceKeyComparer comparer = new NonceKeyComparer();
            private static object dummyItem = new object();
            private static int lowWaterMark = 50;
            private static int minimumNonceLength = 4;

            public NonceCacheImpl(TimeSpan cachingTimeSpan, int maxCachedNonces) : base(lowWaterMark, maxCachedNonces, comparer, PurgingMode.AccessBasedPurge, TimeSpan.FromTicks(cachingTimeSpan.Ticks >> 2), false)
            {
                this.cachingTimeSpan = cachingTimeSpan;
            }

            public bool CheckNonce(byte[] nonce)
            {
                if (nonce == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("nonce");
                }
                if (nonce.Length < minimumNonceLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("NonceLengthTooShort"));
                }
                return (base.GetItem(nonce) != null);
            }

            public bool TryAddNonce(byte[] nonce)
            {
                if (nonce == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("nonce");
                }
                if (nonce.Length < minimumNonceLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("NonceLengthTooShort"));
                }
                DateTime expirationTime = TimeoutHelper.Add(DateTime.UtcNow, this.cachingTimeSpan);
                return base.TryAddItem(nonce, dummyItem, expirationTime, false);
            }

            internal sealed class NonceKeyComparer : IEqualityComparer, IEqualityComparer<byte[]>
            {
                public int Compare(object x, object y)
                {
                    return this.Compare((byte[]) x, (byte[]) y);
                }

                public int Compare(byte[] x, byte[] y)
                {
                    if (object.ReferenceEquals(x, y))
                    {
                        return 0;
                    }
                    if (x != null)
                    {
                        if (y == null)
                        {
                            return 1;
                        }
                        byte[] buffer = x;
                        int length = buffer.Length;
                        byte[] buffer2 = y;
                        int num2 = buffer2.Length;
                        if (length == num2)
                        {
                            for (int i = 0; i < length; i++)
                            {
                                int num4 = buffer[i] - buffer2[i];
                                if (num4 != 0)
                                {
                                    return num4;
                                }
                            }
                            return 0;
                        }
                        if (length > num2)
                        {
                            return 1;
                        }
                    }
                    return -1;
                }

                public bool Equals(object x, object y)
                {
                    return (this.Compare(x, y) == 0);
                }

                public bool Equals(byte[] x, byte[] y)
                {
                    return (this.Compare(x, y) == 0);
                }

                public int GetHashCode(object o)
                {
                    return this.GetHashCode((byte[]) o);
                }

                public int GetHashCode(byte[] o)
                {
                    byte[] buffer = o;
                    return (((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 0x10)) | (buffer[3] << 0x18));
                }
            }
        }
    }
}

