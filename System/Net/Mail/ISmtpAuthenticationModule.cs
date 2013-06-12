namespace System.Net.Mail
{
    using System;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;

    internal interface ISmtpAuthenticationModule
    {
        Authorization Authenticate(string challenge, NetworkCredential credentials, object sessionCookie, string spn, ChannelBinding channelBindingToken);
        void CloseContext(object sessionCookie);

        string AuthenticationType { get; }
    }
}

