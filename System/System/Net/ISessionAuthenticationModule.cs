namespace System.Net
{
    using System;

    internal interface ISessionAuthenticationModule : IAuthenticationModule
    {
        void ClearSession(WebRequest webRequest);
        bool Update(string challenge, WebRequest webRequest);

        bool CanUseDefaultCredentials { get; }
    }
}

