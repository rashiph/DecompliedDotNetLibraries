namespace System.Net
{
    using System;
    using System.Globalization;

    internal class CredentialHostKey
    {
        internal string AuthenticationType;
        internal string Host;
        private bool m_ComputedHashCode;
        private int m_HashCode;
        internal int Port;

        internal CredentialHostKey(string host, int port, string authenticationType)
        {
            this.Host = host;
            this.Port = port;
            this.AuthenticationType = authenticationType;
        }

        public override bool Equals(object comparand)
        {
            CredentialHostKey key = comparand as CredentialHostKey;
            if (comparand == null)
            {
                return false;
            }
            return (((string.Compare(this.AuthenticationType, key.AuthenticationType, StringComparison.OrdinalIgnoreCase) == 0) && (string.Compare(this.Host, key.Host, StringComparison.OrdinalIgnoreCase) == 0)) && (this.Port == key.Port));
        }

        public override int GetHashCode()
        {
            if (!this.m_ComputedHashCode)
            {
                this.m_HashCode = (this.AuthenticationType.ToUpperInvariant().GetHashCode() + this.Host.ToUpperInvariant().GetHashCode()) + this.Port.GetHashCode();
                this.m_ComputedHashCode = true;
            }
            return this.m_HashCode;
        }

        internal bool Match(string host, int port, string authenticationType)
        {
            if ((host == null) || (authenticationType == null))
            {
                return false;
            }
            if (string.Compare(authenticationType, this.AuthenticationType, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }
            if (string.Compare(this.Host, host, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }
            if (port != this.Port)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return ("[" + this.Host.Length.ToString(NumberFormatInfo.InvariantInfo) + "]:" + this.Host + ":" + this.Port.ToString(NumberFormatInfo.InvariantInfo) + ":" + ValidationHelper.ToString(this.AuthenticationType));
        }
    }
}

