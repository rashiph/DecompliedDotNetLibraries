namespace System.Web.Hosting
{
    using System;
    using System.Collections;
    using System.Web;

    public abstract class VirtualDirectory : VirtualFileBase
    {
        protected VirtualDirectory(string virtualPath)
        {
            base._virtualPath = VirtualPath.CreateTrailingSlash(virtualPath);
        }

        public abstract IEnumerable Children { get; }

        public abstract IEnumerable Directories { get; }

        public abstract IEnumerable Files { get; }

        public override bool IsDirectory
        {
            get
            {
                return true;
            }
        }
    }
}

