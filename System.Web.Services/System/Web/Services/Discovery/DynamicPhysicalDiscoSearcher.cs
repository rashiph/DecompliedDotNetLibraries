namespace System.Web.Services.Discovery
{
    using System;
    using System.IO;

    internal class DynamicPhysicalDiscoSearcher : DynamicDiscoSearcher
    {
        private string startDir;

        internal DynamicPhysicalDiscoSearcher(string searchDir, string[] excludedUrls, string startUrl) : base(excludedUrls)
        {
            this.startDir = searchDir;
            base.origUrl = startUrl;
        }

        protected override DirectoryInfo GetPhysicalDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return null;
            }
            DirectoryInfo info = new DirectoryInfo(dir);
            if (!info.Exists)
            {
                return null;
            }
            if ((info.Attributes & (FileAttributes.Temporary | FileAttributes.System | FileAttributes.Hidden)) != 0)
            {
                return null;
            }
            return info;
        }

        protected override string MakeAbsExcludedPath(string pathRelativ)
        {
            return (this.startDir + '\\' + pathRelativ.Replace('/', '\\'));
        }

        protected override string MakeResultPath(string dirName, string fileName)
        {
            return string.Concat(new object[] { base.origUrl, dirName.Substring(this.startDir.Length, dirName.Length - this.startDir.Length).Replace('\\', '/'), '/', fileName });
        }

        internal override void Search(string fileToSkipAtBegin)
        {
            this.SearchInit(fileToSkipAtBegin);
            base.ScanDirectory(this.startDir);
        }

        protected override void SearchSubDirectories(string localDir)
        {
            DirectoryInfo info = new DirectoryInfo(localDir);
            if (info.Exists)
            {
                foreach (DirectoryInfo info2 in info.GetDirectories())
                {
                    if (!(info2.Name == ".") && !(info2.Name == ".."))
                    {
                        base.ScanDirectory(localDir + '\\' + info2.Name);
                    }
                }
            }
        }

        protected override bool IsVirtualSearch
        {
            get
            {
                return false;
            }
        }
    }
}

