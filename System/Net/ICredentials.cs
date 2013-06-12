namespace System.Net
{
    using System;

    public interface ICredentials
    {
        NetworkCredential GetCredential(Uri uri, string authType);
    }
}

