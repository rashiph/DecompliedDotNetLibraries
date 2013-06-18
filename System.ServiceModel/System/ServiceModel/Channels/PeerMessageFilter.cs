namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class PeerMessageFilter
    {
        private Uri actingAs;
        private Uri via;

        public PeerMessageFilter(Uri via) : this(via, null)
        {
        }

        public PeerMessageFilter(Uri via, EndpointAddress to)
        {
            this.via = via;
            if (to != null)
            {
                this.actingAs = to.Uri;
            }
        }

        public bool Match(Uri peerVia, Uri toCond)
        {
            if (peerVia == null)
            {
                return false;
            }
            if (Uri.Compare(this.via, peerVia, UriComponents.Path | UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }
            if (this.actingAs != null)
            {
                return (Uri.Compare(this.actingAs, toCond, UriComponents.Path | UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0);
            }
            return true;
        }
    }
}

