namespace System.Web.UI
{
    using System;

    public interface IResourceUrlGenerator
    {
        string GetResourceUrl(Type type, string resourceName);
    }
}

