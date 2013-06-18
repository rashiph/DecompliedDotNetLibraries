namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;

    public interface IHttpCookieContainerManager
    {
        System.Net.CookieContainer CookieContainer { get; set; }
    }
}

