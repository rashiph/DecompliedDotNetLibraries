namespace System.Web.UI.Design
{
    using System;
    using System.Configuration;
    using System.Runtime.InteropServices;

    [Guid("cff39fa8-5607-4b6d-86f3-cc80b3cfe2dd")]
    public interface IWebApplication : IServiceProvider
    {
        IProjectItem RootProjectItem { get; }
        IProjectItem GetProjectItemFromUrl(string appRelativeUrl);
        System.Configuration.Configuration OpenWebConfiguration(bool isReadOnly);
    }
}

