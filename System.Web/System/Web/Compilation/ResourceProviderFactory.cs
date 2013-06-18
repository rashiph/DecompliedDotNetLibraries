namespace System.Web.Compilation
{
    using System;

    public abstract class ResourceProviderFactory
    {
        protected ResourceProviderFactory()
        {
        }

        public abstract IResourceProvider CreateGlobalResourceProvider(string classKey);
        public abstract IResourceProvider CreateLocalResourceProvider(string virtualPath);
    }
}

