namespace System.Web.Hosting
{
    using System;

    public interface IProcessHostPreloadClient
    {
        void Preload(string[] parameters);
    }
}

