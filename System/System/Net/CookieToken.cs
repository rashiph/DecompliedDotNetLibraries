namespace System.Net
{
    using System;

    internal enum CookieToken
    {
        Nothing,
        NameValuePair,
        Attribute,
        EndToken,
        EndCookie,
        End,
        Equals,
        Comment,
        CommentUrl,
        CookieName,
        Discard,
        Domain,
        Expires,
        MaxAge,
        Path,
        Port,
        Secure,
        HttpOnly,
        Unknown,
        Version
    }
}

