namespace Microsoft.Build.Tasks
{
    using System;
    using System.IO;

    internal class FileState
    {
        private System.IO.FileInfo fileInfo;
        private string filename;

        internal FileState(string filename)
        {
            this.filename = filename;
        }

        internal void Reset()
        {
            this.fileInfo = null;
        }

        internal FileAttributes Attributes
        {
            get
            {
                return this.FileInfo.Attributes;
            }
        }

        internal bool Exists
        {
            get
            {
                return this.FileInfo.Exists;
            }
        }

        private System.IO.FileInfo FileInfo
        {
            get
            {
                if (this.fileInfo == null)
                {
                    this.fileInfo = new System.IO.FileInfo(this.filename);
                }
                return this.fileInfo;
            }
        }

        internal bool IsReadOnly
        {
            get
            {
                return ((this.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
            }
        }

        internal DateTime LastWriteTime
        {
            get
            {
                return this.FileInfo.LastWriteTime;
            }
        }

        internal long Length
        {
            get
            {
                return this.fileInfo.Length;
            }
        }

        internal string Name
        {
            get
            {
                return this.filename;
            }
        }
    }
}

