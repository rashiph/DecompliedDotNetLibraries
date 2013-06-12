namespace System.Web.Hosting
{
    using System;
    using System.Web;
    using System.Web.Configuration;

    [Serializable]
    internal class SimpleConfigMapPathFactory : IConfigMapPathFactory
    {
        IConfigMapPath IConfigMapPathFactory.Create(string virtualPath, string physicalPath)
        {
            WebConfigurationFileMap fileMap = new WebConfigurationFileMap();
            VirtualPath path = VirtualPath.Create(virtualPath);
            fileMap.VirtualDirectories.Add(path.VirtualPathStringNoTrailingSlash, new VirtualDirectoryMapping(physicalPath, true));
            fileMap.VirtualDirectories.Add(HttpRuntime.AspClientScriptVirtualPath, new VirtualDirectoryMapping(HttpRuntime.AspClientScriptPhysicalPathInternal, false));
            return new UserMapPath(fileMap);
        }
    }
}

