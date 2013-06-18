namespace System.Web.UI
{
    using System;

    public interface IUrlResolutionService
    {
        string ResolveClientUrl(string relativeUrl);
    }
}

