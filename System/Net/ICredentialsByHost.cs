namespace System.Net
{
    using System;

    public interface ICredentialsByHost
    {
        NetworkCredential GetCredential(string host, int port, string authenticationType);
    }
}

