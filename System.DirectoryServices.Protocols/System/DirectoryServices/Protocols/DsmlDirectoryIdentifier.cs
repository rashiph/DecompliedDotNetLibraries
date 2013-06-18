namespace System.DirectoryServices.Protocols
{
    using System;

    public class DsmlDirectoryIdentifier : DirectoryIdentifier
    {
        private Uri uri;

        public DsmlDirectoryIdentifier(Uri serverUri)
        {
            if (serverUri == null)
            {
                throw new ArgumentNullException("serverUri");
            }
            if ((string.Compare(serverUri.Scheme, "http", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(serverUri.Scheme, "https", StringComparison.OrdinalIgnoreCase) != 0))
            {
                throw new ArgumentException(Res.GetString("DsmlNonHttpUri"));
            }
            this.uri = serverUri;
        }

        public Uri ServerUri
        {
            get
            {
                return this.uri;
            }
        }
    }
}

