namespace System.Web.Compilation
{
    using System;
    using System.Web;

    internal class ResXResourceProviderFactory : ResourceProviderFactory
    {
        public override IResourceProvider CreateGlobalResourceProvider(string classKey)
        {
            return new GlobalResXResourceProvider(classKey);
        }

        public override IResourceProvider CreateLocalResourceProvider(string virtualPath)
        {
            return new LocalResXResourceProvider(VirtualPath.Create(virtualPath));
        }
    }
}

