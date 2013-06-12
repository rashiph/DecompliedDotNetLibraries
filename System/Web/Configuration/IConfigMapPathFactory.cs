namespace System.Web.Configuration
{
    using System;

    public interface IConfigMapPathFactory
    {
        IConfigMapPath Create(string virtualPath, string physicalPath);
    }
}

