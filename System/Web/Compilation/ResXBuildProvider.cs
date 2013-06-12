namespace System.Web.Compilation
{
    using System;
    using System.IO;
    using System.Resources;
    using System.Web.Hosting;

    internal sealed class ResXBuildProvider : BaseResourcesBuildProvider
    {
        protected override IResourceReader GetResourceReader(Stream inputStream)
        {
            ResXResourceReader reader = new ResXResourceReader(inputStream);
            string path = HostingEnvironment.MapPath(base.VirtualPath);
            reader.BasePath = Path.GetDirectoryName(path);
            return reader;
        }
    }
}

