namespace System.ServiceModel.Security
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class SecurityTimestamp
    {
        private char[] computedCreationTimeUtc;
        private char[] computedExpiryTimeUtc;
        private DateTime creationTimeUtc;
        private const string DefaultFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        internal static readonly TimeSpan defaultTimeToLive = SecurityProtocolFactory.defaultTimestampValidityDuration;
        private readonly byte[] digest;
        private readonly string digestAlgorithm;
        private DateTime expiryTimeUtc;
        private readonly string id;

        public SecurityTimestamp(DateTime creationTimeUtc, DateTime expiryTimeUtc, string id) : this(creationTimeUtc, expiryTimeUtc, id, null, null)
        {
        }

        internal SecurityTimestamp(DateTime creationTimeUtc, DateTime expiryTimeUtc, string id, string digestAlgorithm, byte[] digest)
        {
            if (creationTimeUtc > expiryTimeUtc)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ArgumentOutOfRangeException("recordedExpiryTime", System.ServiceModel.SR.GetString("CreationTimeUtcIsAfterExpiryTime")));
            }
            this.creationTimeUtc = creationTimeUtc;
            this.expiryTimeUtc = expiryTimeUtc;
            this.id = id;
            this.digestAlgorithm = digestAlgorithm;
            this.digest = digest;
        }

        internal char[] GetCreationTimeChars()
        {
            if (this.computedCreationTimeUtc == null)
            {
                this.computedCreationTimeUtc = ToChars(ref this.creationTimeUtc);
            }
            return this.computedCreationTimeUtc;
        }

        internal byte[] GetDigest()
        {
            return this.digest;
        }

        internal char[] GetExpiryTimeChars()
        {
            if (this.computedExpiryTimeUtc == null)
            {
                this.computedExpiryTimeUtc = ToChars(ref this.expiryTimeUtc);
            }
            return this.computedExpiryTimeUtc;
        }

        private static char[] ToChars(ref DateTime utcTime)
        {
            char[] buffer = new char["yyyy-MM-ddTHH:mm:ss.fffZ".Length];
            int offset = 0;
            ToChars(utcTime.Year, buffer, ref offset, 4);
            buffer[offset++] = '-';
            ToChars(utcTime.Month, buffer, ref offset, 2);
            buffer[offset++] = '-';
            ToChars(utcTime.Day, buffer, ref offset, 2);
            buffer[offset++] = 'T';
            ToChars(utcTime.Hour, buffer, ref offset, 2);
            buffer[offset++] = ':';
            ToChars(utcTime.Minute, buffer, ref offset, 2);
            buffer[offset++] = ':';
            ToChars(utcTime.Second, buffer, ref offset, 2);
            buffer[offset++] = '.';
            ToChars(utcTime.Millisecond, buffer, ref offset, 3);
            buffer[offset++] = 'Z';
            return buffer;
        }

        private static void ToChars(int n, char[] buffer, ref int offset, int count)
        {
            for (int i = (offset + count) - 1; i >= offset; i--)
            {
                buffer[i] = (char) (0x30 + (n % 10));
                n /= 10;
            }
            offset += count;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "SecurityTimestamp: Id={0}, CreationTimeUtc={1}, ExpirationTimeUtc={2}", new object[] { this.Id, XmlConvert.ToString(this.CreationTimeUtc, XmlDateTimeSerializationMode.RoundtripKind), XmlConvert.ToString(this.ExpiryTimeUtc, XmlDateTimeSerializationMode.RoundtripKind) });
        }

        internal void ValidateFreshness(TimeSpan timeToLive, TimeSpan allowedClockSkew)
        {
            DateTime utcNow = DateTime.UtcNow;
            if (this.ExpiryTimeUtc <= TimeoutHelper.Subtract(utcNow, allowedClockSkew))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TimeStampHasExpiryTimeInPast", new object[] { this.ExpiryTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.CurrentCulture), utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.CurrentCulture), allowedClockSkew })));
            }
            if (this.CreationTimeUtc >= TimeoutHelper.Add(utcNow, allowedClockSkew))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TimeStampHasCreationTimeInFuture", new object[] { this.CreationTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.CurrentCulture), utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.CurrentCulture), allowedClockSkew })));
            }
            if (this.CreationTimeUtc <= TimeoutHelper.Subtract(utcNow, TimeoutHelper.Add(timeToLive, allowedClockSkew)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TimeStampWasCreatedTooLongAgo", new object[] { this.CreationTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.CurrentCulture), utcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.CurrentCulture), timeToLive, allowedClockSkew })));
            }
        }

        internal void ValidateRangeAndFreshness(TimeSpan timeToLive, TimeSpan allowedClockSkew)
        {
            if (this.CreationTimeUtc >= this.ExpiryTimeUtc)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TimeStampHasCreationAheadOfExpiry", new object[] { this.CreationTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.CurrentCulture), this.ExpiryTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.CurrentCulture) })));
            }
            this.ValidateFreshness(timeToLive, allowedClockSkew);
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                return this.creationTimeUtc;
            }
        }

        public string DigestAlgorithm
        {
            get
            {
                return this.digestAlgorithm;
            }
        }

        public DateTime ExpiryTimeUtc
        {
            get
            {
                return this.expiryTimeUtc;
            }
        }

        public string Id
        {
            get
            {
                return this.id;
            }
        }
    }
}

