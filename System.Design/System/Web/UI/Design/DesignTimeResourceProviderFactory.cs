namespace System.Web.UI.Design
{
    using System;
    using System.Web.Compilation;

    public abstract class DesignTimeResourceProviderFactory
    {
        protected DesignTimeResourceProviderFactory()
        {
        }

        public abstract IResourceProvider CreateDesignTimeGlobalResourceProvider(IServiceProvider serviceProvider, string classKey);
        public abstract IResourceProvider CreateDesignTimeLocalResourceProvider(IServiceProvider serviceProvider);
        public abstract IDesignTimeResourceWriter CreateDesignTimeLocalResourceWriter(IServiceProvider serviceProvider);
    }
}

