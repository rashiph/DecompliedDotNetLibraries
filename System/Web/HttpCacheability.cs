namespace System.Web
{
    using System;

    public enum HttpCacheability
    {
        NoCache = 1,
        Private = 2,
        Public = 4,
        Server = 3,
        ServerAndNoCache = 3,
        ServerAndPrivate = 5
    }
}

