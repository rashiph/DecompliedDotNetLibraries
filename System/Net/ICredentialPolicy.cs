namespace System.Net
{
    using System;

    public interface ICredentialPolicy
    {
        bool ShouldSendCredential(Uri challengeUri, WebRequest request, NetworkCredential credential, IAuthenticationModule authenticationModule);
    }
}

