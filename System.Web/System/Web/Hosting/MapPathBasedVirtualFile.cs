namespace System.Web.Hosting
{
    using System;
    using System.IO;
    using System.Web.Compilation;
    using System.Web.Util;

    internal class MapPathBasedVirtualFile : VirtualFile
    {
        private FindFileData _ffd;
        private string _physicalPath;

        internal MapPathBasedVirtualFile(string virtualPath) : base(virtualPath)
        {
        }

        internal MapPathBasedVirtualFile(string virtualPath, string physicalPath, FindFileData ffd) : base(virtualPath)
        {
            this._physicalPath = physicalPath;
            this._ffd = ffd;
        }

        private void EnsureFileInfoObtained()
        {
            if (this._physicalPath == null)
            {
                this._physicalPath = HostingEnvironment.MapPathInternal(base.VirtualPath);
                FindFileData.FindFile(this._physicalPath, out this._ffd);
            }
        }

        public override Stream Open()
        {
            this.EnsureFileInfoObtained();
            TimeStampChecker.AddFile(base.VirtualPath, this._physicalPath);
            return new FileStream(this._physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public override string Name
        {
            get
            {
                this.EnsureFileInfoObtained();
                if (this._ffd == null)
                {
                    return base.Name;
                }
                return this._ffd.FileNameLong;
            }
        }

        internal string PhysicalPath
        {
            get
            {
                this.EnsureFileInfoObtained();
                return this._physicalPath;
            }
        }
    }
}

