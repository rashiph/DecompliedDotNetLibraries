namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class HttpAnonymousUriPrefixMatcher : IAnonymousUriPrefixMatcher
    {
        private UriPrefixTable<Uri> anonymousUriPrefixes;

        internal HttpAnonymousUriPrefixMatcher()
        {
        }

        internal HttpAnonymousUriPrefixMatcher(HttpAnonymousUriPrefixMatcher objectToClone) : this()
        {
            if (objectToClone.anonymousUriPrefixes != null)
            {
                this.anonymousUriPrefixes = new UriPrefixTable<Uri>(objectToClone.anonymousUriPrefixes);
            }
        }

        internal bool IsAnonymousUri(Uri to)
        {
            Uri uri;
            if (this.anonymousUriPrefixes == null)
            {
                return false;
            }
            return this.anonymousUriPrefixes.TryLookupUri(to, HostNameComparisonMode.Exact, out uri);
        }

        public void Register(Uri anonymousUriPrefix)
        {
            if (anonymousUriPrefix == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("anonymousUriPrefix");
            }
            if (!anonymousUriPrefix.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("anonymousUriPrefix", System.ServiceModel.SR.GetString("UriMustBeAbsolute"));
            }
            if (this.anonymousUriPrefixes == null)
            {
                this.anonymousUriPrefixes = new UriPrefixTable<Uri>(true);
            }
            if (!this.anonymousUriPrefixes.IsRegistered(new BaseUriWithWildcard(anonymousUriPrefix, HostNameComparisonMode.Exact)))
            {
                this.anonymousUriPrefixes.RegisterUri(anonymousUriPrefix, HostNameComparisonMode.Exact, anonymousUriPrefix);
            }
        }
    }
}

