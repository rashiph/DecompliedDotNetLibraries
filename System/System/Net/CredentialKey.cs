namespace System.Net
{
    using System;
    using System.Globalization;

    internal class CredentialKey
    {
        internal string AuthenticationType;
        private bool m_ComputedHashCode;
        private int m_HashCode;
        internal Uri UriPrefix;
        internal int UriPrefixLength = -1;

        internal CredentialKey(Uri uriPrefix, string authenticationType)
        {
            this.UriPrefix = uriPrefix;
            this.UriPrefixLength = this.UriPrefix.ToString().Length;
            this.AuthenticationType = authenticationType;
        }

        public override bool Equals(object comparand)
        {
            CredentialKey key = comparand as CredentialKey;
            if (comparand == null)
            {
                return false;
            }
            return ((string.Compare(this.AuthenticationType, key.AuthenticationType, StringComparison.OrdinalIgnoreCase) == 0) && this.UriPrefix.Equals(key.UriPrefix));
        }

        public override int GetHashCode()
        {
            if (!this.m_ComputedHashCode)
            {
                this.m_HashCode = (this.AuthenticationType.ToUpperInvariant().GetHashCode() + this.UriPrefixLength) + this.UriPrefix.GetHashCode();
                this.m_ComputedHashCode = true;
            }
            return this.m_HashCode;
        }

        internal bool IsPrefix(Uri uri, Uri prefixUri)
        {
            if (((prefixUri.Scheme != uri.Scheme) || (prefixUri.Host != uri.Host)) || (prefixUri.Port != uri.Port))
            {
                return false;
            }
            int length = prefixUri.AbsolutePath.LastIndexOf('/');
            if (length > uri.AbsolutePath.LastIndexOf('/'))
            {
                return false;
            }
            return (string.Compare(uri.AbsolutePath, 0, prefixUri.AbsolutePath, 0, length, StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal bool Match(Uri uri, string authenticationType)
        {
            if ((uri == null) || (authenticationType == null))
            {
                return false;
            }
            if (string.Compare(authenticationType, this.AuthenticationType, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }
            return this.IsPrefix(uri, this.UriPrefix);
        }

        public override string ToString()
        {
            return ("[" + this.UriPrefixLength.ToString(NumberFormatInfo.InvariantInfo) + "]:" + ValidationHelper.ToString(this.UriPrefix) + ":" + ValidationHelper.ToString(this.AuthenticationType));
        }
    }
}

