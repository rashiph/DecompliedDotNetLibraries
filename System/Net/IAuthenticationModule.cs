namespace System.Net
{
    using System;

    public interface IAuthenticationModule
    {
        Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials);
        Authorization PreAuthenticate(WebRequest request, ICredentials credentials);

        string AuthenticationType { get; }

        bool CanPreAuthenticate { get; }
    }
}

