namespace System.Deployment.Application
{
    using System;
    using System.IO;
    using System.Threading;

    internal class TempDirectory : DisposableBase
    {
        private const uint _directorySegmentCount = 2;
        private string _thePath;

        public TempDirectory() : this(System.IO.Path.GetTempPath())
        {
        }

        public TempDirectory(string basePath)
        {
            do
            {
                this._thePath = System.IO.Path.Combine(basePath, System.Deployment.Application.PathHelper.GenerateRandomPath(2));
            }
            while (Directory.Exists(this._thePath) || File.Exists(this._thePath));
            Directory.CreateDirectory(this._thePath);
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

