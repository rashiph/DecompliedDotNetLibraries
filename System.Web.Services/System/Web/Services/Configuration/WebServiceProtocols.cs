namespace System.Web.Services.Configuration
{
    using System;

    [Flags]
    public enum WebServiceProtocols
    {
        AnyHttpSoap = 0x21,
        Documentation = 8,
        HttpGet = 2,
        HttpPost = 4,
        HttpPostLocalhost = 0x10,
        HttpSoap = 1,
        HttpSoap12 = 0x20,
        Unknown = 0
    }
}

