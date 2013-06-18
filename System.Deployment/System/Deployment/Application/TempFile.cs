namespace System.Deployment.Application
{
    using System;
    using System.IO;
    using System.Threading;

    internal class TempFile : DisposableBase
    {
        private const uint _filePathSegmentCount = 2;
        private string _thePath;

        public TempFile() : this(System.IO.Path.GetTempPath())
        {
        }

        public TempFile(string basePath) : this(basePath, string.Empty)
        {
        }

        public TempFile(string basePath, string suffix)
        {
            do
            {
                this._thePath = System.IO.Path.Combine(basePath, System.Deployment.Application.PathHelper.GenerateRandomPath(2) + suffix);
            }
            while (File.Exists(this._thePath) || Directory.Exists(this._thePath));
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this._thePath));
        }

        protected override void DisposeUnmanagedResources()
        {
            string rootSegmentPath = System.Deployment.Application.PathHelper.GetRootSegmentPath(this._thePath, 2);
            if (Directory.Exists(rootSegmentPath))
            {
                try
                {
                    Directory.Delete(rootSegmentPath, true);
                }
                catch (IOException)
                {
                    Thread.Sleep(10);
                    try
                    {
                        Directory.Delete(rootSegmentPath, true);
                    }
                    catch (IOException)
                    {
                    }
                }
            }
        }

        public string Path
        {
            get
            {
                return this._thePath;
            }
        }
    }
}

