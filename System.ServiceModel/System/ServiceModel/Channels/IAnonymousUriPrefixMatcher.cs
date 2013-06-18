namespace System.ServiceModel.Channels
{
    using System;

    public interface IAnonymousUriPrefixMatcher
    {
        void Register(Uri anonymousUriPrefix);
    }
}

