namespace System.Web.Configuration
{
    using System;

    internal interface IServerConfig2
    {
        bool IsWithinApp(string virtualPath);
    }
}

