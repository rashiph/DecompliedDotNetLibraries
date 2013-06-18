namespace System.Web
{
    using System;

    public interface IHttpModule
    {
        void Dispose();
        void Init(HttpApplication context);
    }
}

