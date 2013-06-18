namespace System.ServiceModel.Channels
{
    using System;

    internal abstract class MsmqBindingFilter
    {
        private MsmqUri.IAddressTranslator addressing;
        private string prefix;

        public MsmqBindingFilter(string path, MsmqUri.IAddressTranslator addressing)
        {
            this.prefix = path;
            this.addressing = addressing;
            if ((this.prefix.Length > 0) && (this.prefix[0] == '/'))
            {
                this.prefix = this.prefix.Substring(1);
            }
            if ((this.prefix.Length > 0) && (this.prefix[this.prefix.Length - 1] != '/'))
            {
                this.prefix = this.prefix + '/';
            }
        }

        public Uri CreateServiceUri(string host, string name, bool isPrivate)
        {
            return this.addressing.CreateUri(host, name, isPrivate);
        }

        public int Match(string name)
        {
            if (string.Compare(this.CanonicalPrefix, 0, name, 0, this.CanonicalPrefix.Length, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.CanonicalPrefix.Length;
            }
            return -1;
        }

        public abstract object MatchFound(string host, string name, bool isPrivate);
        public abstract void MatchLost(string host, string name, bool isPrivate, object callbackState);

        public string CanonicalPrefix
        {
            get
            {
                return this.prefix;
            }
        }
    }
}

