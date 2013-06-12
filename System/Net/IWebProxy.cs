namespace System.Net
{
    using System;

    public interface IWebProxy
    {
        Uri GetProxy(Uri destination);
        bool IsBypassed(Uri host);

        ICredentials Credentials { get; set; }
    }
}

