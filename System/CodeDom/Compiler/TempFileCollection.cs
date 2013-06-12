namespace System.CodeDom.Compiler
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;

    [Serializable, PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class TempFileCollection : ICollection, IEnumerable, IDisposable
    {
        private string basePath;
        private Hashtable files;
        private bool keepFiles;
        private string tempDir;

        public TempFileCollection() : this(null, false)
        {
        }

        public TempFileCollection(string tempDir) : this(tempDir, false)
        {
        }

        public TempFileCollection(string tempDir, bool keepFiles)
        {
            this.keepFiles = keepFiles;
            this.tempDir = tempDir;
            this.files = new Hashtable(StringComparer.OrdinalIgnoreCase);
        }

        public string AddExtension(string fileExtension)
        {
            return this.AddExtension(fileExtension, this.keepFiles);
        }

        public string AddExtension(string fileExtension, bool keepFile)
        {
            if ((fileExtension == null) || (fileExtension.Length == 0))
            {
                throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", new object[] { "fileExtension" }), "fileExtension");
            }
            string fileName = this.BasePath + "." + fileExtension;
            this.AddFile(fileName, keepFile);
            return fileName;
        }

        public void AddFile(string fileName, bool keepFile)
        {
            if ((fileName == null) || (fileName.Length == 0))
            {
                throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", new object[] { "fileName" }), "fileName");
            }
            if (this.files[fileName] != null)
            {
                throw new ArgumentException(SR.GetString("DuplicateFileName", new object[] { fileName }), "fileName");
            }
            this.files.Add(fileName, keepFile);
        }

        public void CopyTo(string[] fileNames, int start)
        {
            this.files.Keys.CopyTo(fileNames, start);
        }

        public void Delete()
        {
            if ((this.files != null) && (this.files.Count > 0))
            {
                string[] array = new string[this.files.Count];
                this.files.Keys.CopyTo(array, 0);
                foreach (string str in array)
                {
                    if (!this.KeepFile(str))
                    {
                        this.Delete(str);
                        this.files.Remove(str);
                    }
                }
            }
        }

        private void Delete(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch
            {
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Delete();
        }

        private void EnsureTempNameCreated()
        {
            if (this.basePath == null)
            {
                string path = null;
                bool flag = false;
                int num = 0x1388;
                do
                {
                    try
                    {
                        this.basePath = GetTempFileName(this.TempDir);
                        string fullPath = Path.GetFullPath(this.basePath);
                        new FileIOPermission(FileIOPermissionAccess.AllAccess, fullPath).Demand();
                        path = this.basePath + ".tmp";
                        using (new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                        {
                        }
                        flag = true;
                    }
                    catch (IOException exception)
                    {
                        num--;
                        uint num2 = 0x80070050;
                        if ((num == 0) || (Marshal.GetHRForException(exception) != num2))
                        {
                            throw;
                        }
                        flag = false;
                    }
                }
                while (!flag);
                this.files.Add(path, this.keepFiles);
            }
        }

        ~TempFileCollection()
        {
            this.Dispose(false);
        }

        public IEnumerator GetEnumerator()
        {
            return this.files.Keys.GetEnumerator();
        }

        private static string GetTempFileName(string tempDir)
        {
            if (string.IsNullOrEmpty(tempDir))
            {
                tempDir = Path.GetTempPath();
            }
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            if (tempDir.EndsWith(@"\", StringComparison.Ordinal))
            {
                return (tempDir + fileNameWithoutExtension);
            }
            return (tempDir + @"\" + fileNameWithoutExtension);
        }

        private bool KeepFile(string fileName)
        {
            object obj2 = this.files[fileName];
            if (obj2 == null)
            {
                return false;
            }
            return (bool) obj2;
        }

        internal void SafeDelete()
        {
            WindowsImpersonationContext impersonation = Executor.RevertImpersonation();
            try
            {
                this.Delete();
            }
            finally
            {
                Executor.ReImpersonate(impersonation);
            }
        }

        void ICollection.CopyTo(Array array, int start)
        {
            this.files.Keys.CopyTo(array, start);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.files.Keys.GetEnumerator();
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string BasePath
        {
            get
            {
                this.EnsureTempNameCreated();
                return this.basePath;
            }
        }

        public int Count
        {
            get
            {
                return this.files.Count;
            }
        }

        public bool KeepFiles
        {
            get
            {
                return this.keepFiles;
            }
            set
            {
                this.keepFiles = value;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.files.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return null;
            }
        }

        public string TempDir
        {
            get
            {
                if (this.tempDir != null)
                {
                    return this.tempDir;
                }
                return string.Empty;
            }
        }
    }
}

