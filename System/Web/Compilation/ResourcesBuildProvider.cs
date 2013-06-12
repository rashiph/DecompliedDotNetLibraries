namespace System.Web.Compilation
{
    using System.IO;
    using System.Resources;

    internal class ResourcesBuildProvider : BaseResourcesBuildProvider
    {
        protected override IResourceReader GetResourceReader(Stream inputStream)
        {
            return new ResourceReader(inputStream);
        }
    }
}

