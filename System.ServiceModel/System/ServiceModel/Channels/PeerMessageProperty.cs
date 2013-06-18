namespace System.ServiceModel.Channels
{
    using System;

    internal class PeerMessageProperty
    {
        public int CacheMiss;
        public bool MessageVerified;
        public Uri PeerTo;
        public Uri PeerVia;
        public bool SkipLocalChannels;
    }
}

