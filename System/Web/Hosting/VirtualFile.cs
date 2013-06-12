namespace System.Web.Hosting
{
    using System;
    using System.IO;
    using System.Web;

    public abstract class VirtualFile : VirtualFileBase
    {
        protected VirtualFile(string virtualPath)
        {
            base._virtualPath = VirtualPath.Create(virtualPath);
        }

        public abstract Stream Open();

        public override bool IsDirectory
        {
            get
            {
                return false;
            }
        }
    }
}

