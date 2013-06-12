namespace System.Web.Compilation
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Security.Permissions;
    using System.Web;

    internal class LocalResXResourceProvider : BaseResXResourceProvider
    {
        private VirtualPath _virtualPath;

        internal LocalResXResourceProvider(VirtualPath virtualPath)
        {
            this._virtualPath = virtualPath;
        }

        protected override ResourceManager CreateResourceManager()
        {
            Assembly localResourceAssembly = this.GetLocalResourceAssembly();
            if (localResourceAssembly == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ResourceExpresionBuilder_PageResourceNotFound"));
            }
            return new ResourceManager(this._virtualPath.FileName, localResourceAssembly) { IgnoreCase = true };
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private Assembly GetLocalResourceAssembly()
        {
            BuildResult buildResultFromCache = BuildManager.GetBuildResultFromCache(BuildManager.GetLocalResourcesAssemblyName(this._virtualPath.Parent));
            if (buildResultFromCache != null)
            {
                return ((BuildResultCompiledAssembly) buildResultFromCache).ResultAssembly;
            }
            return null;
        }

        public override IResourceReader ResourceReader
        {
            get
            {
                Assembly localResourceAssembly = this.GetLocalResourceAssembly();
                if (localResourceAssembly == null)
                {
                    return null;
                }
                string name = (this._virtualPath.FileName + ".resources").ToLower(CultureInfo.InvariantCulture);
                Stream manifestResourceStream = localResourceAssembly.GetManifestResourceStream(name);
                if (manifestResourceStream == null)
                {
                    return null;
                }
                return new System.Resources.ResourceReader(manifestResourceStream);
            }
        }
    }
}

