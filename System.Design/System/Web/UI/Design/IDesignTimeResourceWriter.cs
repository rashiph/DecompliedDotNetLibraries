namespace System.Web.UI.Design
{
    using System;
    using System.Resources;

    public interface IDesignTimeResourceWriter : IResourceWriter, IDisposable
    {
        string CreateResourceKey(string resourceName, object obj);
    }
}

