namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Util;

    internal class GlobalPageThemeBuildProvider : PageThemeBuildProvider
    {
        private VirtualPath _virtualDirPath;

        internal GlobalPageThemeBuildProvider(VirtualPath virtualDirPath) : base(virtualDirPath)
        {
            this._virtualDirPath = virtualDirPath;
        }

        internal override string AssemblyNamePrefix
        {
            get
            {
                return "App_GlobalTheme_";
            }
        }

        public override ICollection VirtualPathDependencies
        {
            get
            {
                ICollection virtualPathDependencies = base.VirtualPathDependencies;
                string fileName = this._virtualDirPath.FileName;
                CaseInsensitiveStringSet set = new CaseInsensitiveStringSet();
                set.AddCollection(virtualPathDependencies);
                string o = UrlPath.SimpleCombine(HttpRuntime.AppDomainAppVirtualPathString, "App_Themes");
                string virtualDir = o + '/' + fileName;
                if (HostingEnvironment.VirtualPathProvider.DirectoryExists(virtualDir))
                {
                    set.Add(virtualDir);
                    return set;
                }
                set.Add(o);
                return set;
            }
        }
    }
}

